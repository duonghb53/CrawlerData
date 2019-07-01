using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CrawlerData
{
    public partial class Form1 : Form
    {
        //private const string WEB_URL = @"https://vnexpress.net/suc-khoe/dau-lung-nen-nam-nem-cung-hay-mem-3944694.html";
        //private const string WEB_URL = @"https://dantri.com.vn/suc-khoe/nguoi-nha-benh-nhan-say-xin-dam-vao-mat-nu-bac-si-20190627112323245.htm";
        private const string WEB_URL = @"https://vnexpress.net/suc-khoe/be-trai-2-tuoi-bi-suy-dinh-duong-vi-tac-ta-trang-3945669.html";
        private const string PATTERN_DANTRI = @"<p>(?<title>[^<]*)<\/[pP]>";
        private const string PATTERN_VNEX = @"<section class=""sidebar_1"">(.*)</section>";
        //private const string PATTERN_LINK = @"<a href=\"".*\"">(?<name>.*)</a>";
        private const string PATTERN_LINK = @"https://vnexpress.net/suc-khoe/(.*?).html";
        private const string path = @"Link_cao.txt";
        private const string Outfile = @"content";


        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            textBox1.Text = String.Empty;
            try
            {
                string[] lines = File.ReadAllLines(path);
                string[] links = lines.Distinct().ToArray();
                StreamWriter sw = File.CreateText(Outfile + DateTime.Now.ToString("_yyyyMMddHHmmss") + ".txt");
                foreach (string lnk in links)
                {
                    string result = String.Empty;
                    string data = String.Empty;

                    data = ReadTextFromUrl(lnk);
                    result = GetContentNews(data, PATTERN_VNEX);
                    result = GetPlainTextFromHtml(result);
                    sw.WriteLine(result);
                    sw.WriteLine(Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("button1_Click: " + ex.ToString());
                return;
            }


            //textBox1.Text = result;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                textBox1.Text = String.Empty;
                string[] lines = File.ReadAllLines(textBox2.Text);
                string[] links = lines.Distinct().ToArray();
                StreamWriter sw = File.AppendText(path);
                int dem = 0;

                foreach (string lnk in links)
                {
                    List<string> result = new List<string>();
                    string data = String.Empty;
                    if (textBox2.Text.Length == 0)
                    {
                        MessageBox.Show("Nhap link web");
                        return;
                    }
                    data = ReadTextFromUrl(lnk);
                    result = GetLink(data, PATTERN_LINK);
                    // This text is added only once to the file.

                    List<string> distinct = result.Distinct().ToList();

                    foreach (string link in distinct)
                    {
                        sw.WriteLine(link);
                        dem++;
                    }
                }
                MessageBox.Show("Lay duoc " + dem.ToString() + " links");

                sw.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("button2_Click: " + ex.ToString());
                return;
            }

        }

        string ReadTextFromUrl(string url)
        {
            try
            {
                // WebClient is still convenient
                // Assume UTF8, but detect BOM - could also honor response charset I suppose
                using (var client = new WebClient())
                using (var stream = client.OpenRead(url))
                using (var textReader = new StreamReader(stream, Encoding.UTF8, true))
                {
                    return textReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetContentNews: " + ex.ToString());
                return null;
            }

        }

        public string GetContentNews(string data, string pattern)
        {
            string result = String.Empty;
            try
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase|RegexOptions.Singleline);
                MatchCollection matches = regex.Matches(data);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            result += match.Value.ToString() + Environment.NewLine;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetContentNews: " + ex.ToString());
                return null;
            }
            return result;
        }


        public  List<string> GetLink(string data, string pattern)
        {
            List<string> result = new List<string>();
            try
            {
                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(data);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (match.Success)
                        {
                            result.Add(match.Value.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetLink: " + ex.ToString());
                return null;
            }
            return result;
        }

        private string GetPlainTextFromHtml(string htmlString)
        {
            try
            {
                string htmlTagPattern = "<.*?>";
                var regexCss = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
                htmlString = regexCss.Replace(htmlString, string.Empty);
                htmlString = Regex.Replace(htmlString, htmlTagPattern, string.Empty);
                htmlString = Regex.Replace(htmlString, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                htmlString = htmlString.Replace("&nbsp;", string.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("GetPlainTextFromHtml: " + ex.ToString());
                return null;
            }
            return htmlString;
        }

        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }
    } 
}
