using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using testArmo.Properties;
using System.Diagnostics;
using System.Threading;

namespace testArmo
{
    public partial class Form1 : Form
    {
        ManualResetEventSlim mres = new ManualResetEventSlim(true);
        public Form1()
        {
            InitializeComponent();
            textBox_fileName.Text = Settings.Default["fileNameText"].ToString();
            textBox_directory.Text = Settings.Default["startDirectoryText"].ToString();
            textBox_text.Text = Settings.Default["insertText"].ToString();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Thread.Sleep(0);
            treeView1.Nodes.Clear();
            label9.Text = "";
            label11.Text = "";
            Settings.Default["fileNameText"] = textBox_fileName.Text;
            Settings.Default["startDirectoryText"] = textBox_directory.Text;
            Settings.Default["insertText"] = textBox_text.Text;
            Settings.Default.Save();
            String fileName = textBox_fileName.Text;
            String startDirectory = textBox_directory.Text;
            String insertText = textBox_text.Text;
            if (textBox_directory.Text != "" && textBox_fileName.Text != "" && textBox_text.Text == "")
            {
                label5.Text = fileName;
                FindAllFilesAsync(startDirectory, fileName);
            }
            else if (textBox_directory.Text != "" && textBox_text.Text != "" && textBox_fileName.Text == "")
            {
                FindInTextFileAsync(startDirectory, insertText);
            }
            else if (textBox_directory.Text != "" && textBox_text.Text != "" && textBox_fileName.Text != "")
            {
                MessageBox.Show("Выберете только 1 параметр поиска", "Ошибка");
            }


        }

        async void FindAllFilesAsync(string startDirectory, string fileName)
        {
            await Task<string>.Run(() => FindAllFiles(startDirectory, fileName));
        }

        async void FindInTextFileAsync(string startDirectory, string insertText)
        {
            await Task<string>.Run(() => FindInTextFile(startDirectory, insertText));
        }
        private void FindInTextFile(string startDirectory, string insertText)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<string> files = new List<string>();
            foreach (var s in Directory.GetFiles(startDirectory, "*", SearchOption.AllDirectories)) //перебираем по файлам
            {

                Action<string> action3 = currentFileInText;
                if (InvokeRequired)
                {
                    Invoke(action3, s);
                }
                else
                {
                    action3(s);
                }
                mres.Wait();
                string text = File.ReadAllText(s, Encoding.GetEncoding("Windows-1251"));
                if (text.Contains(insertText))
                {
                    Debug.WriteLine("asd");
                    files.Add(s);
                }

            }
            string watch = stopwatch.ElapsedMilliseconds.ToString();
            string countFiles = Directory.GetFiles(startDirectory, "*", SearchOption.AllDirectories).Length.ToString();
            Action<string, string, List<string>> actionBuildTree = buildTree;
            if (InvokeRequired)
            {
                Invoke(actionBuildTree, watch, countFiles, files);
            }
            else
            {
                actionBuildTree(watch, countFiles, files);
            }
            Thread.Sleep(0);
        }

        void DirSearch(string sDir, string fileName, ref List<string> files)
        {
            Debug.WriteLine("asd");
            foreach (string d in Directory.GetDirectories(sDir))
            {

                Action<string> action2 = currentFile;
                if (InvokeRequired)
                {
                    Invoke(action2, d);
                }
                else
                {
                    action2(d);
                }
                mres.Wait();
                foreach (string f in Directory.GetFiles(d, fileName+ ".*"))     
                {
                    Action<List<string>> action = updateTree;
                    files.Add(f);
                    if (InvokeRequired)
                    {
                        Invoke(action, files);
                    }
                    else
                    {
                        action(files);
                    }
                }
                DirSearch(d, fileName, ref files);
            }


        }
        private void currentFileInText(string s)
        {

            label7.Text = s;
        }
        private void currentFile(string d)
        {
            string[] fileProcessing = Directory.GetFiles(d);
            foreach (string fi in fileProcessing)
            {
                label7.Text = fi;
            }
        }
        private void updateTree(List<string> files)
        {
            treeView1.PathSeparator = @"\";
            PopulateTreeView(treeView1, files, '\\');
        }
        private void buildTree(string watch, string countFiles, List<string> files)
        {
            label11.Text = watch;
            label9.Text = countFiles;
            treeView1.PathSeparator = @"\";
            PopulateTreeView(treeView1, files, '\\');
        }
        void FindAllFiles(string startDirectory, string fileName)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<string> files = new List<string>();
            DirSearch(startDirectory, fileName, ref files);
            stopwatch.Stop();
            string countFiles = Directory.GetFiles(startDirectory, "*", SearchOption.AllDirectories).Length.ToString();
            string watch = stopwatch.ElapsedMilliseconds.ToString();
            Action<string, string, List<string>> action = buildTree;
            if (InvokeRequired)
            {
                Invoke(action, watch, countFiles, files);
            }
            else
            {
                action(watch, countFiles, files);
            }
        }

        void FileProcessing(string startDirectory)
        {
            foreach (string file in Directory.GetFiles(startDirectory, "*", SearchOption.AllDirectories))
            {
                label7.Text = file;
            }
            label7.Text = "";
        }


        private static void PopulateTreeView(TreeView treeView, IEnumerable<string> paths, char pathSeparator)
        {
            TreeNode lastNode = null;
            string subPathAgg;
            foreach (string path in paths)
            {
                subPathAgg = string.Empty;
                foreach (string subPath in path.Split(pathSeparator))
                {
                    subPathAgg += subPath + pathSeparator;
                    TreeNode[] nodes = treeView.Nodes.Find(subPathAgg, true);
                    if (nodes.Length == 0)
                        if (lastNode == null)
                            lastNode = treeView.Nodes.Add(subPathAgg, subPath);
                        else
                            lastNode = lastNode.Nodes.Add(subPathAgg, subPath);
                    else
                        lastNode = nodes[0];
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if(mres.IsSet)
            {
                mres.Reset();
                button2.Text = "Продолжить";
            }
            else
            {
                mres.Set();
                button2.Text = "Стоп";
            }
        }
    }
}

 
