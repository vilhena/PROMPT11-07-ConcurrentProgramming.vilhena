using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Timer = System.Windows.Forms.Timer;

namespace FileFinder
{
    public partial class FileFinderForm : Form
    {
        public FileFinderForm()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            String fldr = txtFolder.Text;
            if (String.IsNullOrWhiteSpace(fldr) || !Directory.Exists(fldr))
            {
                MessageBox.Show("Please indicate a valid base folder");
                return;
            }

            String patt = txtPattern.Text;
            if (String.IsNullOrWhiteSpace(patt))
            {
                MessageBox.Show("Please indicate a valid pattern");
                return;
            }

            String word = txtWord.Text;
            if (String.IsNullOrWhiteSpace(word))
            {
                MessageBox.Show("Please indicate a word");
                return;
            }

            btnSearch.Enabled = false;
            txtResults.Text = "";
            Task.Factory.StartNew(() =>
                                      {

                                          Parallel.ForEach(
                                              Directory.EnumerateDirectories(fldr, "*", SearchOption.AllDirectories)
                                              , (dname) =>
                                                    {

                                                        var nameList = new ConcurrentQueue<string>();

                                                        Parallel.ForEach(
                                                            Directory.EnumerateFiles(dname, patt),
                                                            (nname) =>
                                                                {
                                                                    String name = nname;
                                                                    try
                                                                    {
                                                                        String content = File.ReadAllText(name);
                                                                        if (content.Contains(word))
                                                                        {
                                                                            nameList.Enqueue(name);
                                                                        }
                                                                    }
                                                                    catch
                                                                    {
                                                                    }

                                                                });

                                                        var list = nameList;


                                                        //if (list.Count > 100)
                                                        //{
                                                        //    var newList = new ConcurrentQueue<string>();

                                                        //    if (Interlocked.CompareExchange(ref nameList, newList, list) == list)
                                                        //    {
                                                        
                                                        //}
                                                        //;


                                                        if (nameList.Count > 0)
                                                            txtResults.BeginInvoke(
                                                                new Action(
                                                                    () => txtResults.AddLines(nameList)));
                                                    });

                                          //    }
                                      })
                .ContinueWith((t) =>
                                  {
                                      btnSearch.Enabled = true;
                                      MessageBox.Show(TextBoxExtensions.Count.ToString());
                                      MessageBox.Show((TextBoxExtensions.Count / TextBoxExtensions.Calls).ToString());
                                  }, TaskScheduler.FromCurrentSynchronizationContext());


        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select Folder";
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderDialog.SelectedPath;
            }
 
        }
    }

    static class TextBoxExtensions
    {
        public static int Count { get; set; }

        public static int Calls { get; set; }


        public static void AddLine(this TextBox textBox, String text)
        {
            textBox.AppendText(text);
            textBox.AppendText("\r\n");
        }
        public static void AddLines(this TextBox textBox, ConcurrentQueue<string> texts)
        {
            var counter = 0;
            Count += texts.Count;
            ++Calls;
            while (texts.Count > 0)
            {
                string name = "";
                texts.TryDequeue(out name);

                textBox.AddLine(name);

                if (++counter > 10)
                {
                    new System.Threading.Timer((_) =>
                                               textBox.BeginInvoke(new Action(() => textBox.AddLines(texts))), null, 200,
                                               0);
                    return;
                }

            }
        } 
    }
}
