using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace play_mp3
{
    /// <summary>
    /// Логика взаимодействия для History.xaml
    /// </summary>
    public partial class History : Window
    {

        public FileInfo SelectedFile { get; private set; }
        private List<FileInfo> fileList;

        public History(List<FileInfo> files)
        {
            Title = "History of listening";
            fileList = files;

            InitializeComponent();

            foreach (FileInfo file in fileList)
            {
                Button button = new Button
                {
                    Content = file.Name,
                    Tag = file
                };
                button.Click += Song_Click;
                pesenki.Children.Add(button);
            }
        }

        private void Song_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button != null && button.Tag is FileInfo)
            {
                SelectedFile = button.Tag as FileInfo;
                DialogResult = true;
            }
        }
    }
}
