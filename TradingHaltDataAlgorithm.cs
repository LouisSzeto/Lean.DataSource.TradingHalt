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

using QuantConnect.Data;
using QuantConnect.Algorithm;
using QuantConnect.DataSource;

namespace QuantConnect.DataLibrary.Tests
{
    /// <summary>
    /// Example algorithm using the trading halt data
    /// </summary>
    public class TradingHaltDataAlgorithm : QCAlgorithm
    {
        private Symbol _adap, _eftr;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2021, 9, 6);  //Set Start Date
            SetEndDate(2021, 9, 9);    //Set End Date

            _adap = AddEquity("ADAP", Resolution.Second).Symbol;
            _eftr = AddEquity("EFTR", Resolution.Second).Symbol;
            AddData<TradingHalt>(_adap, Resolution.Second);
            AddData<TradingHalt>(_eftr, Resolution.Second);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="slice">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice slice)
        {
            var data = slice.Get<TradingHalt>().Values;
            foreach (var halt in data)
            {
                Log(halt.ToString());
            }
        }
    }
}
