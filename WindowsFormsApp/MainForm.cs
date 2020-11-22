using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class MainForm : Form
    {
        readonly string listPath = @"E:\workspace\sources\nmpa-data\药品\2020\进口药品\列表";
        readonly string detailPath = @"E:\workspace\sources\nmpa-data\药品\2020\进口药品\详情";
        readonly string contentDiv = "<div id='content'></div>";
        readonly string baseUrl = "http://app1.nmpa.gov.cn/data_nmpa/face3/base.jsp";

        string[] files;
        HtmlElementCollection hrefElements;
        int currentFileIndex = -1;
        int currentHrefIndex = -1;

        public MainForm()
        {
            InitializeComponent();

            this.webBrowser.Url = new System.Uri(baseUrl);
            files = Directory.GetFiles(listPath, "*.html");
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (files == null || files.Length < 1)
            {
                Console.WriteLine("没有列表文件");
                return;
            }

            // 
            Thread.Sleep(5000);

            clickHref();

            // 启动定时器
            this.timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("正在处理文件[{0}]链接[{1}]", currentFileIndex, currentHrefIndex);

            HtmlDocument doc = this.webBrowser.Document;
            HtmlElement contentDivEle = doc.GetElementById("content");
            if (contentDivEle == null || string.IsNullOrWhiteSpace(contentDivEle.InnerHtml))
            {
                Console.WriteLine("等待文件[{0}]链接[{1}]的详情数据", currentFileIndex, currentHrefIndex);
                return;
            }

            // 保存详情
            var content = contentDivEle.InnerHtml;
            if (content.Length > 100)
            {
                string path = getDetailPath();
                File.WriteAllText(path, content, Encoding.UTF8);
                Console.WriteLine("保存详情到{0}：{1}", path, content.Length);
            }

            // 重置内容块的InnerHtml
            contentDivEle.InnerHtml = "";

            // 继续下一个
            clickHref();
        }

        private void clickHref()
        {
            while (true)
            {
                ++currentHrefIndex;
                if (hrefElements == null || currentHrefIndex >= hrefElements.Count)
                {
                    // 当前文件所有a标签已处理完，继续下一个文件
                    Console.WriteLine("已处理文件[{0}]，总链接[{1}]", currentFileIndex, currentHrefIndex);

                    ++currentFileIndex;
                    if (currentFileIndex >= files.Length)
                    {
                        // 所有文件已处理完毕
                        this.timer.Stop();
                        Console.WriteLine("所有文件[{0}]处理完毕，停止定时器", currentFileIndex);
                        return;
                    }

                    // 继续处理下一个列表文件 
                    this.webBrowser.Document.Body.InnerHtml = File.ReadAllText(files[currentFileIndex], Encoding.UTF8) + contentDiv;
                    hrefElements = this.webBrowser.Document.Body.GetElementsByTagName("a");
                    currentHrefIndex = 0;
                }

                if (!File.Exists(getDetailPath()))
                {
                    break;
                }
            }

            // 继续处理下一个a标签
            hrefElements[currentHrefIndex].InvokeMember("Click");
        }

        private string getDetailPath()
        {
            string href = hrefElements[currentHrefIndex].GetAttribute("href");
            int start = href.IndexOf("?") + 1;
            int end = href.LastIndexOf("'");
            return Path.Combine(detailPath, href.Substring(start, end - start) + ".html");
        }
    }
}
