import os
import time

from bs4 import BeautifulSoup
from selenium import webdriver

list_path = "药品/国产药品/列表"
detail_path = "药品/国产药品/详情"
base_url = 'http://app1.sfda.gov.cn/datasearchcnda/face3/'

for file in os.listdir(list_path):
    if not file.endswith(".html"):
        continue

    file = os.path.join(list_path, file)
    with open(file, encoding='utf-8') as html_file:
        content = html_file.read()

    soup = BeautifulSoup(content, features="html5lib")
    print("start to process file: %s" % file)
    for a in soup.findAll("a"):
        href = a.get("href")
        i1 = href.find("'")
        i2 = href.find("'", i1 + 1)
        href = href[i1 + 1:i2]

        detail_file = os.path.join(detail_path, href[href.find("?") + 1:]) + ".html"
        if os.path.exists(detail_file):
            continue

        #
        options = webdriver.ChromeOptions()
        options.add_argument("--headless")
        options.add_argument("--disable-gpu")
        options.add_argument("--window-size=1920x1080")
        options.add_argument("--disable-xss-auditor")
        options.add_argument('--disable-logging')
        options.add_argument("--dns-prefetch-disable")
        try:
            with webdriver.Chrome(executable_path="D:/Anaconda3/chromedriver.exe", options=options) as driver:
                driver.get(base_url + href)
                driver.implicitly_wait(2)
                content = driver.page_source
        except:
            print("Unexpected error")
            time.sleep(1)
            continue

        with open(detail_file, mode="w", encoding='utf-8') as html_file:
            html_file.write(content)

        print("save html file[%s]" % detail_file)
        time.sleep(1)

    print("process file[%s] end" % file)
