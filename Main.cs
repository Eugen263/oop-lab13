using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileExplorer
{
    public partial class MainForm : Form
    {
        private string currentPath = "C:\\"; // поточний шлях
        private List<string> drives = new List<string>(); // список дисків
        private List<string> fileExtensions = new List<string> { ".txt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".gif" }; // список підтримуваних розширень файлів

        public MainForm()
        {
            InitializeComponent();
            LoadDrives(); // завантаження списку дисків при старті програми
            LoadFileSystem(currentPath); // завантаження файлової системи
        }

        private void LoadDrives()
        {
            DriveInfo[] driveInfo = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveInfo)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    drives.Add(drive.Name);
                }
            }
        }

        private void LoadFileSystem(string path)
        {
            currentPath = path;
            PathTextBox.Text = currentPath;

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();

            FileListView.Items.Clear();
            DirectoryListView.Items.Clear();

            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                if (fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    DirectoryListView.Items.Add(fileSystemInfo.Name);
                }
                else if (fileExtensions.Contains(fileSystemInfo.Extension.ToLower()))
                {
                    FileListView.Items.Add(fileSystemInfo.Name);
                }
            }

            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(currentPath));
            DriveTypeLabel.Text = "Drive Type: " + driveInfo.DriveType.ToString();
            TotalSizeLabel.Text = "Total Size: " + driveInfo.TotalSize.ToString();
            FreeSpaceLabel.Text = "Free Space: " + driveInfo.AvailableFreeSpace.ToString();
        }

        private void FileListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FileListView.SelectedItems.Count == 1)
            {
                string filePath = Path.Combine(currentPath, FileListView.SelectedItems[0].Text);
                FileInfo fileInfo = new FileInfo(filePath);

                FileTypeLabel.Text = "File Type: " + fileInfo.Extension;
                FileSizeLabel.Text = "File Size: " + fileInfo.Length.ToString();
                LastAccessLabel.Text = "Last Access Time: " + fileInfo.LastAccessTime.ToString();
                LastWriteLabel.Text = "Last Write Time: " + fileInfo.LastWriteTime.ToString();

                if (fileExtensions.Contains(fileInfo.Extension.ToLower()))
                {
                    try
                    {
                        pictureBox.Image = Image.FromFile(filePath);
                    }
                    catch (Exception)
                    {
                        pictureBox.Image = null;
                    }
                }
                else
                {
                    pictureBox.Image = null;
                }
            }
            else
            {
                FileTypeLabel.Text = "";
                FileSizeLabel.Text = "";
                LastAccessLabel.Text = "";
                LastWriteLabel.Text = "";
                pictureBox.Image = null;
            }
        }

        private void DirectoryListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DirectoryListView.SelectedItems.Count == 1)
            {
                string directoryPath = Path.Combine(currentPath, DirectoryListView.SelectedItems[0].Text);
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                FileTypeLabel.Text = "File Type: Folder";
                FileSizeLabel.Text = "";
                LastAccessLabel.Text = "Last Access Time: " + directoryInfo.LastAccessTime.ToString();
                LastWriteLabel.Text = "Last Write Time: " + directoryInfo.LastWriteTime.ToString();

                FileContentTextBox.Text = "";
            }
            else
            {
                FileTypeLabel.Text = "";
                FileSizeLabel.Text = "";
                LastAccessLabel.Text = "";
                LastWriteLabel.Text = "";
                FileContentTextBox.Text = "";
            }
        }

        private void PathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string path = PathTextBox.Text;

                if (Directory.Exists(path))
                {
                    LoadFileSystem(path);
                }
                else
                {
                    MessageBox.Show("Invalid path");
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            string filter = FilterTextBox.Text.ToLower();

            FileListView.Items.Clear();
            DirectoryListView.Items.Clear();

            DirectoryInfo directoryInfo = new DirectoryInfo(currentPath);
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();

            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                if (fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (fileSystemInfo.Name.ToLower().Contains(filter))
                    {
                        DirectoryListView.Items.Add(fileSystemInfo.Name);
                    }
                }
                else if (fileExtensions.Contains(fileSystemInfo.Extension.ToLower()))
                {
                    if (fileSystemInfo.Name.ToLower().Contains(filter))
                    {
                        FileListView.Items.Add(fileSystemInfo.Name);
                    }
                }
            }
        }

        private void FilterDirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            string filter = FilterDirectoryTextBox.Text.ToLower();

            DirectoryListView.Items.Clear();

            DirectoryInfo directoryInfo = new DirectoryInfo(currentPath);
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();

            foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
            {
                if (fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (fileSystemInfo.Name.ToLower().Contains(filter))
                    {
                        DirectoryListView.Items.Add(fileSystemInfo.Name);
                    }
                }
            }
        }
        // перегляд вмісту текстових файлів
        private void ViewTextFile(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);
            TextPreviewForm textPreviewForm = new TextPreviewForm(fileContent);
            textPreviewForm.ShowDialog();
        }
        // фільтрація списку файлів
        private void FilterFiles(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (ListViewItem item in FileListView.Items)
                {
                    item.ForeColor = Color.Black;
                }
            }
            else
            {
                foreach (ListViewItem item in FileListView.Items)
                {
                    if (!item.Text.Contains(searchText))
                    {
                        item.ForeColor = Color.Gray;
                    }
                    else
                    {
                        item.ForeColor = Color.Black;
                    }
                }
            }
        }

        // фільтрація списку каталогів
        private void FilterDirectories(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (ListViewItem item in DirectoryListView.Items)
                {
                    item.ForeColor = Color.Black;
                }
            }
            else
            {
                foreach (ListViewItem item in DirectoryListView.Items)
                {
                    if (!item.Text.Contains(searchText))
                    {
                        item.ForeColor = Color.Gray;
                    }
                    else
                    {
                        item.ForeColor = Color.Black;
                    }
                }
            }
        }
    }
}

// Форма попереднього перегляду текстового файлу
public partial class TextPreviewForm : Form
{
    public TextPreviewForm(string text)
    {
        InitializeComponent();
        TextPreviewTextBox.Text = text;
    }
}

