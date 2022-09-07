# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

from AlgorithmImports import *

### <summary>
### Example algorithm using the trading halt data
### </summary>
class TradingHaltDataAlgorithm(QCAlgorithm):
    def Initialize(self):
        ''' Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.'''
        
        self.SetStartDate(2021, 9, 6)   #Set Start Date
        self.SetEndDate(2021, 9, 9)    #Set End Date
        
        self.adap = self.AddEquity("ADAP", Resolution.Second).Symbol
        self.eftr = self.AddEquity("EFTR", Resolution.Second).Symbol
        self.AddData(TradingHalt, self.adap, Resolution.Second)
        self.AddData(TradingHalt, self.eftr, Resolution.Second)

    def OnData(self, slice):
        ''' OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.

        :param Slice slice: Slice object keyed by symbol containing the stock data
        '''
        data = slice.Get(TradingHalt).Values
        for halt in data:
            self.Log(halt.ToString())