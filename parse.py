import os
import json

from bs4 import BeautifulSoup

detail_path = "药品/国产药品/详情"

data = []

for file in os.listdir(detail_path):
    if not file.endswith(".html"):
        continue

    file = os.path.join(detail_path, file)
    with open(file, encoding='utf-8') as html_file:
        content = html_file.read()

    soup = BeautifulSoup(content, features="html5lib")
    item = {}
    for tr in soup.find_all("table", width="100%", align="center", limit=1)[0].find_all("tr")[1:]:
        td = tr.find_all("td", limit=2)
        item[td[0].get_text().strip()] = td[1].get_text().strip()
    data.append(item)

print(len(data))
with open("国产药品.json", mode="w", encoding='utf-8') as json_file:
    json_file.write(json.dumps(data, ensure_ascii=False))
