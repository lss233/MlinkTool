using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MklinkTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //listBox1.SelectedItem;
            //DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, paths), DragDropEffects.Link);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if(sender is Button)
            {
                return;
            }
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
            }
        }

        private void listbox1_DragDrop(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    foreach(var file in files)
                    {
                        if (!listBox1.Items.Contains(file))
                            listBox.Items.Add(file);
                    }
                } else
                {
                    if (!listBox1.Items.Contains(path))
                        listBox.Items.Add(path);
                }
                
            }
                
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.listBox1.Items.Clear();
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            for(int i = 0; i < this.listBox1.Items.Count; i++)
            {
                this.listBox1.SetSelected(i, true);
            }
        }

        public async Task<int> GetTargetHandle()
        {
            int currrentHandle = GetForegroundWindow().ToInt32();
            Console.WriteLine("Starting parser");
            var taskResult = await Task.Run(() =>
            {
                int handle = 0;
                while(true)
                {
                    handle = GetForegroundWindow().ToInt32();
                    if (handle != currrentHandle && handle > 0)
                    {
                        return handle;
                    }
                    Thread.Sleep(100);
                }
            });
            return taskResult;
        }
        
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [STAThread]
        public static string GetWindowPath(int handle, dynamic windows)
        {
            //Default value for the variable returned at the end of the method if no path was found
            String Path = "";

            //SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();
            foreach (SHDocVw.InternetExplorer window in windows)
            {
                Console.WriteLine(window.LocationURL);
                //If Window handle corresponds to passed handle
                if (window.HWND == handle)
                {
                    //Print path of the respective folder and save it within the returned variable
                    Console.WriteLine(window.LocationURL);
                    Path = window.LocationURL;
                    break;
                }
            }
            //If a path was found, retun the path, otherwise return ""
            return Path;
        }

        private void buttonMklink_Click(object sender, EventArgs e)
        {
            if(this.listBox1.SelectedItems.Count < 1)
            {
                MessageBox.Show("You need to select files first!", "You fucking NERDS", MessageBoxButtons.OK);
                return;
            }
            using (var fbd = new CommonOpenFileDialog { IsFolderPicker = true })
            {
                CommonFileDialogResult result = fbd.ShowDialog();

                if (result == CommonFileDialogResult.Ok)
                {
                    string[] paths = new string[this.listBox1.SelectedItems.Count];
                    for (int i = 0; i < listBox1.SelectedItems.Count; i++)
                    {
                        string file = (string)this.listBox1.SelectedItems[i];
                        paths[i] = "\\\"" + file + "\\\"";
                        paths[i] = "\"" + file + "\"";
                    }
                    
                    foreach (string path in paths)
                    {
                        string[] slash = path.Split("\\".ToCharArray());
                        string pathFileName = slash[slash.Length - 1];
                        if (checkBoxAddPrefix.Checked)
                        {
                            pathFileName = "linked-" + pathFileName;
                        }
                        
                        string target = "\"" + fbd.FileName +"\\" + pathFileName;
                        Process process = new Process();
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        startInfo.WorkingDirectory = @"C:\Windows\System32";
                        startInfo.FileName = "cmd.exe";
                        startInfo.Arguments = "/user:Administrator \"cmd /C mklink " +  target + " " + path + " >> logs.txt\"";
                        Console.WriteLine(startInfo.Arguments);
                        process.StartInfo = startInfo;
                        process.Start();
                    }
                }
            }
            //var shell = new Shell32.Shell();
            //shell.MinimizeAll();
            //var windows = shell.Windows();

            //foreach (SHDocVw.InternetExplorer window in windows) {
            //    Console.WriteLine("Current handle = " + window.FullName);
            //}
            //    GetTargetHandle().ContinueWith(handle => {
            //    Console.WriteLine("Current handle = " + handle.Result);
            //    GetWindowPath(handle.Result, windows);
            //});

            //buttonMklink.DoDragDrop(new DataObject(DataFormats.FileDrop, paths),
            //    DragDropEffects.Link);
        }
    }
}
