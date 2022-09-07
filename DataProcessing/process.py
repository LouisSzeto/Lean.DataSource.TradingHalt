# CLRImports is required to handle Lean C# objects for Mapped Datasets (Single asset and Universe Selection)
from CLRImports import *
from pathlib import Path
from urllib.request import urlopen

# To generate the Security Identifier, we need to create and initialize the Map File Provider
# and call the SecurityIdentifier.GenerateEquity method
mapFileProvider = LocalZipMapFileProvider()
mapFileProvider.Initialize(DefaultDataProvider())

#PROCESS_DATE = os.environ['QC_DATAFLEET_DEPLOYMENT_DATE']
#REQUEST_DATE = datetime.strptime(PROCESS_DATE, '%Y%m%d').strftime('%Y-%m-%d')
REQUEST_URL = f'https://www.nyse.com/api/trade-halts/historical/download?haltDateFrom=2019-01-01&haltDateTo='

INPUT_DATETIME_FORMAT = '%m/%d/%Y %H:%M:%S'
OUTPUT_DATETIME_FORMAT = '%Y%m%d %H:%M:%S'

OUTPUT_DATA_PATH = Path('/temp-output-directory') / 'equity' / 'usa' / 'halt'
OUTPUT_DATA_PATH.mkdir(parents=True, exist_ok=True)

lines = {}

for datum in urlopen(REQUEST_URL).readlines()[1:]:
    items = datum.replace(b'\n', b'').replace(b'\r', b'').decode('utf-8', 'ignore')
    if not items: continue
    items = items.split(',')
    
    halt_start_datetime = datetime.strptime(' '.join(items[:2]), INPUT_DATETIME_FORMAT)
    halt_start = halt_start_datetime.strftime(OUTPUT_DATETIME_FORMAT)
    halt_end = '' if all([not x for x in items[-2:]]) else datetime.strptime(' '.join(items[-2:]), INPUT_DATETIME_FORMAT).strftime(OUTPUT_DATETIME_FORMAT)
    symbol = items[2].replace(' PR', 'PR').replace(' ', '.')     # STR WS -> STR.WS, USB PRM -> USDPRM (all PR* are with no . in LEAN)
    sid = SecurityIdentifier.GenerateEquity(symbol, Market.USA, True, mapFileProvider, halt_start_datetime)

    # for enums
    if items[-3] == "Corporate action":
        reason = 0
    elif items[-3] == "LULD pause":
        reason = 1
    elif items[-3] == "Merger effective":
        reason = 2
    elif items[-3] == "New security offering":
        reason = 3
    elif items[-3] == "News released":
        reason = 4
    elif items[-3] == "News dissemination":
        reason = 5
    elif items[-3] == "News pending":
        reason = 6
    elif items[-3] == "Regulatory concern":
        reason = 7
    
    # sid,symbol,reason,halt_start,halt_end
    line = f'{sid},{symbol},{reason},{halt_start},{halt_end}'
    
    symbol = Symbol(sid, symbol)
    if symbol not in lines:
        lines[symbol] = []
    lines[symbol].append(line)

for symbol, new_data in lines.items():
    existing = []
    csv_path = OUTPUT_DATA_PATH / f'{symbol.Value.lower()}.csv'
    
    if os.path.exists(csv_path):
        with open(csv_path, 'r', encoding='UTF-8') as csv:
            existing = [line.replace('\n', '').replace('\r', '') for line in csv.readlines()]
    
    data = set(existing + new_data)
    with open(csv_path, 'w', encoding='UTF-8') as csv:
        csv.write('\n'.join(data))