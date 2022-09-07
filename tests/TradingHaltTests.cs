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
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.DataSource;

namespace QuantConnect.DataLibrary.Tests
{
    [TestFixture]
    public class TradingHaltTests
    {
        [Test]
        public void JsonRoundTrip()
        {
            var expected = CreateNewInstance();
            var type = expected.GetType();
            var serialized = JsonConvert.SerializeObject(expected);
            var result = JsonConvert.DeserializeObject(serialized, type);

            AssertAreEqual(expected, result);
        }

        [Test]
        public void Clone()
        {
            var expected = CreateNewInstance();
            var result = expected.Clone();

            AssertAreEqual(expected, result);
        }

        [Test]
        public void ReaderTest()
        {
            var expected = CreateNewCollectionInstance();
            var factory = new TradingHalt();
            var result = factory.Reader(null, " ,,1,20200101 10:31:02,20200102 14:32:56", DateTime.Now, false);

            AssertAreEqual(expected, result);
        }

        private void AssertAreEqual(object expected, object result, bool filterByCustomAttributes = false)
        {
            foreach (var propertyInfo in expected.GetType().GetProperties())
            {
                // we skip Symbol which isn't protobuffed
                if (filterByCustomAttributes && propertyInfo.CustomAttributes.Count() != 0)
                {
                    Assert.AreEqual(propertyInfo.GetValue(expected), propertyInfo.GetValue(result));
                }
            }
            foreach (var fieldInfo in expected.GetType().GetFields())
            {
                Assert.AreEqual(fieldInfo.GetValue(expected), fieldInfo.GetValue(result));
            }
        }

        private BaseData CreateNewInstance()
        {
            return new TradingHalt
            {
                Symbol = Symbol.Empty,
                Time = DateTime.Today,
                EndTime = DateTime.Today,
                Reason = HaltReason.NewsReleased,
                Flag = HaltFlag.Start
            };
        }

        private BaseDataCollection CreateNewCollectionInstance()
        {
            var haltStart = Parse.DateTimeExact("20200101 10:31:02", "yyyyMMdd HH:mm:ss");
            var haltEnd = Parse.DateTimeExact("20200102 14:32:56", "yyyyMMdd HH:mm:ss");
            var symbol = Symbol.Empty;
            var reason = HaltReason.LULDPause;

            var data = new List<TradingHalt>
            {
                new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.Start,
                    Reason = reason,
                    Time = haltStart,
                    EndTime = haltStart
                },
                new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.End,
                    Reason = reason,
                    Time = haltStart,
                    EndTime = new DateTime(2020, 1, 1, 20, 0, 0)
                },
                new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.Start,
                    Reason = reason,
                    Time = new DateTime(2020, 1, 2, 4, 0, 0),
                    EndTime = new DateTime(2020, 1, 2, 4, 0, 0)
                },
                new TradingHalt
                {
                    Symbol = symbol,
                    Flag = HaltFlag.Start,
                    Reason = reason,
                    Time = new DateTime(2020, 1, 2, 4, 0, 0),
                    EndTime = haltEnd
                },
            };

            return new BaseDataCollection(haltStart, haltEnd, symbol, data);
        }
    }
}