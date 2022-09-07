/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Collections.Generic;
using System.IO;
using NodaTime;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.DataSource
{
    /// <summary>
    /// Example custom data type
    /// </summary>
    public class TradingHalt : BaseData
    {
        /// <summary>
        /// Reason of trading halt
        /// </summary>
        public HaltReason Reason { get; set; }

        /// <summary>
        /// Signal flag of trading halt
        /// </summary>
        public HaltFlag Flag { get; set; }

        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "equity",
                    "usa",
                    "halt",
                    $"{config.Symbol.Value.ToLowerInvariant()}.csv"
                ),
                SubscriptionTransportMedium.LocalFile,
                FileFormat.UnfoldingCollection
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            var csv = line.Split(',');

            var symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]);
            var reason = (HaltReason)Enum.Parse(typeof(HaltReason), csv[2], true);
            var haltStart = Parse.DateTimeExact(csv[3], "yyyyMMdd HH:mm:ss");
            var haltEnd = string.IsNullOrEmpty(csv[4]) ? DateTime.Now : Parse.DateTimeExact(csv[4], "yyyyMMdd HH:mm:ss");

            var data = new List<TradingHalt>();
            TimeSpan ts = new TimeSpan(4, 0, 0);                // consider pre market hour
            
            // provide daily flag so algorithm starts in the middle of cross-date halting can also be notified
            // e.g. halt from day1 10am to day2 3pm, there will be a total of 4 flags to be emitted:
            // 1. day1 10am halt start
            // 2. day1 8pm halt end (day1 post market hour end)
            // 3. day2 4am halt start (day2 pre market hour start)
            // 4. day2 4am halt end
            // So user get notified even if algorithm starts on day2
            for (DateTime time = haltStart; time < haltEnd; time = time.AddDays(1).Date + ts)
            {
                // Signal start
                data.Add(new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.Start,
                    Reason = reason,
                    Time = time,
                    EndTime = time
                });

                // Signal end
                data.Add(new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.End,
                    Reason = reason,
                    Time = time,
                    EndTime = time.Date == haltEnd.Date ?
                        haltEnd :
                        time.Date + new TimeSpan(20, 0, 0)      // consider post market hour
                });
            }

            return new BaseDataCollection(haltStart, haltEnd, symbol, data);
        }

        /// <summary>
        /// Indicates whether the data source is tied to an underlying symbol and requires that corporate events be applied to it as well, such as renames and delistings
        /// </summary>
        /// <returns>false</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Converts the instance to string
        /// </summary>
        public override string ToString()
        {
            return $"{Symbol} - {EndTime} - {Flag} - {Reason}";
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Second;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return AllResolutions;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return TimeZones.NewYork;
        }
    }
}
