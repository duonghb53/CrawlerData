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
        private const string PATTERN_DANTRI = @"<p>(?<title>[^<]*)<\/[pP]>";
        private const string PATTERN_VNEX = @"<section class=""sidebar_1"">(.*?)</section>";
        private const string PATTERN_WHO = @"<div id =""content"">(.*?)</div>";
        private const string PATTERN_MOH = @"<div id =""contentDetail"">(.*?)</div>";
        
        private const string PATTERN_LINK_EX = @"https://vnexpress.net/suc-khoe/(.*?).html";
        private const string PATTERN_LINK_DT = @"https://vnexpress.net/suc-khoe/(.*?).html";
        private const string PRE_LINK_WHO = @"http://www.wpro.who.int";
        private const string PATTERN_LINK_WHO = "<a href=\"(?<name>.*)/vi/index.html\">";
        private const string PATTERN_LINK_WHO_VI = "<a href=\"(?<name>.*)/vi\">";
        private const string PATTERN_LINK_MOH = "<a href=\"https://www.moh.gov.vn/(.*?)\">";



        private const string path = @"Link_cao.txt";
        private const string Outfile = @"Yte_SKhoe";


        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add("WHO");
            comboBox1.Items.Add("MOH");
            comboBox1.Items.Add("VNEX");
            comboBox1.Items.Add("DANTRI");
        }

        private void button1_Click(object sender, EventArgs e)
        {


            textBox1.Text = String.Empty;
            try
            {
                string[] lines = File.ReadAllLines(path);
                string[] links = lines.Distinct().ToArray();
                string fileName = Outfile + DateTime.Now.ToString("_yyyyMMddHHmmss") + ".txt";
                using (StreamWriter sw = new StreamWriter(fileName))
                { 
                    foreach (string lnk in links)
                    {
                        string result = String.Empty;
                        string data = String.Empty;

                        data = ReadTextFromUrl(lnk);
                        int case_web = comboBox1.SelectedIndex;
                        string pattern;
                        switch(case_web)
                        {
                            case 0:
                                pattern = PATTERN_WHO;
                                break;
                            case 1:
                                pattern = PATTERN_MOH;
                                break;
                            case 2:
                                pattern = PATTERN_VNEX;
                                break;
                            case 3:
                                pattern = PATTERN_DANTRI;
                                break;
                            default:
                                return;
                        }

                        result = GetContentNews(data, pattern);
                        result = GetPlainTextFromHtml(result);
                        sw.WriteLine(result);
                        textBox1.Text = result;
                    }
                    sw.Close();
                }
                MessageBox.Show("Lay duoc " + links.Length.ToString() + " bai");

            }
            catch (Exception ex)
            {
                MessageBox.Show("button1_Click: " + ex.ToString());
                return;
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length == 0)
            {
                MessageBox.Show("Nhap link web");
                return;
            }
            try
            {
                textBox1.Text = String.Empty;
                string[] lines = File.ReadAllLines(textBox2.Text);
                string[] links = lines.Distinct().ToArray();
                StreamWriter sw = File.AppendText(path);
                List<string> totalLink = new List<string>();
                foreach (string lnk in links)
                {
                    List<string> result = new List<string>();
                    string data = String.Empty;
                    
                    data = ReadTextFromUrl(lnk);
                    int case_web = comboBox1.SelectedIndex;
                    string pattern;
                    switch (case_web)
                    {
                        case 0:
                            pattern = PATTERN_LINK_WHO;
                            break;
                        case 1:
                            pattern = PATTERN_LINK_MOH;
                            break;
                        case 2:
                            pattern = PATTERN_LINK_EX;
                            break;
                        case 3:
                            pattern = PATTERN_LINK_DT;
                            break;
                        default:
                            return;
                    }
                    result = GetLink(data, pattern);
                    totalLink.AddRange(result);
                }
                List<string> distinct = totalLink.Distinct().ToList();
                foreach (string link in distinct)
                {
                    sw.WriteLine(link);
                }
                MessageBox.Show("Lay duoc " + distinct.Count.ToString() + " links");

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
                Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
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
                            string link = match.Value.ToString().Replace("<a href=\"", string.Empty);
                            link = link.Replace("\">", string.Empty);                           
                            result.Add(PRE_LINK_WHO + link);
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
                htmlString = htmlString.Replace("\n", " ");
                htmlString = htmlString.Replace("\t", string.Empty);
                htmlString = htmlString.Replace("  ", string.Empty);
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
