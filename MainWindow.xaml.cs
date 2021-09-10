using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Duplicate_Files
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        List<(string, string)> Dir_File = new List<(string, string)>();

        List<string > Dir_List = new List<string >();
        /// <summary>
        /// Initializes all GUI components
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ListView_Files.SelectionMode = System.Windows.Controls.SelectionMode.Single;
        }
        /// <summary>
        /// Handles users choise of a directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Choose_Dir_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                string d_path = dialog.SelectedPath;

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(d_path))
                {
                    if (Dir_List.Count > 0)
                    {
                        if (Dir_List.Contains(d_path))
                        {
                            MessageBox.Show("Directory is already in the list");
                            return;
                        }
                    }
                        
                    Dir_List.Add(d_path);
                    Dir_File_item dir_item = new Dir_File_item(Dir_List.Count, d_path);
                    ListView_Dir.Items.Add(dir_item);
                }
            }
        }

        /// <summary>
        /// Starts search for duplicates according to attributes chosen by the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Search_Click(object sender, RoutedEventArgs e)
        {
            Dir_File.Clear();
            ListView_Files.Items.Clear();

            if (Dir_List.Count == 0)
            {
                MessageBox.Show("At least one directory should be choosen.");
                return;
            }

            bool Size_Checked = (bool)CheckBox_Size.IsChecked;
            bool Date_Checked = (bool)CheckBox_Date.IsChecked;
            bool Name_Checked = (bool)CheckBox_Name.IsChecked;

            if (!Size_Checked & !Date_Checked & !Name_Checked)
            {
                MessageBox.Show("At least one similarity attribute should be choosen.");
                return;
            }

            List<string> All_files = new List<string>();
            List<string> files_tmp = new List<string>();
            List<long> files_size_tmp = new List<long>();
            List<long> All_files_size = new List<long>(); 
            List<DateTime> files_date_tmp = new List<DateTime>();
            List<DateTime> All_files_date = new List<DateTime>();

            List<int> All_files_dir_nr = new List<int>();

            int count = 0;

            List<(string Dir_Path, string File_Name, long Size, DateTime f_date)> Dir_File_tmp;

            foreach(string dir_path in Dir_List)
            {
                
                files_tmp = (new DirectoryInfo(dir_path)).GetFiles(".").Select(a => a.FullName).ToList();
                All_files.AddRange(files_tmp);

                var current_dir_nr = Enumerable.Repeat(count + 1, files_tmp.Count).ToList();
                All_files_dir_nr.AddRange(current_dir_nr);

                files_size_tmp = (new DirectoryInfo(dir_path)).GetFiles(".").Select(a => a.Length).ToList();
                All_files_size.AddRange(files_size_tmp);

                files_date_tmp = (new DirectoryInfo(dir_path)).GetFiles(".").Select(a => a.LastWriteTime).ToList();
                All_files_date.AddRange(files_date_tmp);

                count++;
            }

            int N_files = All_files.Count();

            if (N_files <= 0)
            {
                MessageBox.Show("Less than 2 files are found in chosen directories. Duplicate search cannot be done.");
                return;
            }

            bool duplicates_found = false;
            int i = 0;
            int[] duplicates_index_name = Enumerable.Range(0, All_files_size.Count).ToArray();
            int[] duplicates_index_size = Enumerable.Range(0, All_files_size.Count).ToArray();
            int[] duplicates_index_date = Enumerable.Range(0, All_files_size.Count).ToArray();
            int[] duplicates_index = new int[0];

            while ((!duplicates_found) & (i < N_files))
            {
                if (Size_Checked)
                {
                    
                    var newList = Enumerable.Range(0, All_files_size.Count).Where(j => All_files_size[j] == All_files_size[i]).ToList();

                    if (newList.Count > -1)
                    {
                        //duplicates_found = true;
                        duplicates_index_size = newList.ToArray();
                    }
                    
                }

                if (Date_Checked)
                {

                    var newList = Enumerable.Range(0, All_files_date.Count).Where(j => All_files_date[j] == All_files_date[i]).ToList();

                    if (newList.Count > -1)
                    {
                        //duplicates_found = true;
                        duplicates_index_date = newList.ToArray();
                    }

                }

                if (Name_Checked)
                {

                    var newList = Enumerable.Range(0, All_files.Count).Where(j => System.IO.Path.GetFileName(All_files[j]) == System.IO.Path.GetFileName(All_files[i])).ToList();

                    if (newList.Count > -1)
                    {
                        //duplicates_found = true;
                        duplicates_index_name = newList.ToArray();
                    }

                }

                duplicates_index_size.Intersect(duplicates_index_date).ToArray();
                duplicates_index = duplicates_index_size.Intersect(duplicates_index_name).ToArray();

                if (duplicates_index.Length > 1)
                {
                    duplicates_found = true;
                }

                i++;

            }

            if (duplicates_found)
            {
                for (int j = 0; j < duplicates_index.Length; j++)
                {
                    Dir_File.Add((System.IO.Path.GetDirectoryName(All_files[duplicates_index[j]]) , System.IO.Path.GetFileName(All_files[duplicates_index[j]]) ));

                    Dir_File_item item_tmp = new Dir_File_item(All_files_dir_nr[duplicates_index[j]] , Dir_File[j].Item2);

                    ListView_Files.Items.Add(item_tmp);
                }
            }


        }
        /// <summary>
        /// Deletes duplicates if any
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (Dir_List.Count == 0)
            {
                return;
            }
            if (Dir_File.Count == 0)
            {
                return;
            }

            if(ListView_Files.Items.Count < 2)
            {
                return;
            }

            int selected_index = ListView_Files.SelectedIndex;

            if (selected_index < 0)
            {
                MessageBox.Show("Select one file which should be kept.");
                return;
            }

            Dir_File_item selected_item = (Dir_File_item)ListView_Files.SelectedItem;
           
            for (int i = 0; i < ListView_Files.Items.Count; i++)
            {
                string file_path = Dir_File[i].Item1 + System.IO.Path.DirectorySeparatorChar + Dir_File[i].Item2;
                if (i != selected_index)
                {
                    //MessageBox.Show("Deleting file: "  + file_path);
                    File.Delete(file_path);
                }
            }

            ListView_Files.Items.Clear();
            Dir_File.Clear();

            ListView_Files.Items.Add(selected_item);

            CheckBox_Date.IsChecked = false;
            CheckBox_Size.IsChecked = false;
            CheckBox_Name.IsChecked = false;

        }
  
        /// <summary>
        /// Clears all choises and updates GUI components in order to start over
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Clear_Click(object sender, RoutedEventArgs e)
        {
            Dir_List = new List<string>();
            Dir_File = new List<(string Dir_Path, string File_Name)>();

            ListView_Dir.Items.Clear();
            ListView_Files.Items.Clear();

            CheckBox_Date.IsChecked = false;
            CheckBox_Size.IsChecked = false;
            CheckBox_Name.IsChecked = false;
        }

        private void Btn_Unmark_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    /// <summary>
    /// Class for items in ListViews
    /// </summary>
    public class Dir_File_item
    {
        public int Dir_Nr { set; get; }
        public string File_Name { set; get; }
        
        public Dir_File_item(int col_nr, string col_dir)
        {
            Dir_Nr = col_nr;
            File_Name = col_dir;
            
        }
    }
}
