using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace System_Programming_Task3;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private string copyAddress;
    private long fileSize;
    private long progress = 0;
    private int percentage;
    private bool cancel = false;
    private int? password;
    private string defaultText = "";

    public int Percentage { get => percentage; set { percentage = value; OnPropertyChanged(); } }
    public long ProgressValue { get => progress; set { progress = value; OnPropertyChanged(); } }
    public long FileSize { get => fileSize; set { fileSize = value; OnPropertyChanged(); } }
    public int? Password { get => password; set { password = value; OnPropertyChanged(); } }
    public string CopyAddress { get => copyAddress; set { copyAddress = value; OnPropertyChanged(); } }
    public MainWindow()
    {
        InitializeComponent();
        ProgressValue = 0;
        FileSize = 100;
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button is null) return;

        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        string selectedFilePath = "";
        if (openFileDialog.ShowDialog() == true)
            selectedFilePath = openFileDialog.FileName;
        CopyAddress = selectedFilePath;
        ProgressValue = 0;
        Percentage = 0;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        cancel = true;
        if (ProgressValue > 0 && Percentage < 100)
        {
            File.WriteAllText(CopyAddress, defaultText);
            Percentage = 0;
            ProgressValue = 0;
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(CopyAddress))
            {
                MessageBox.Show("File Path is not correct", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!(Password is int a)) return;
            var text = File.ReadAllText(CopyAddress);
            defaultText = text;
            FileSize = text.Length;
            ProgressValue = 0;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var temp = text.ToCharArray();
                for (int i = 0; i < text.Length; i++)
                {
                    if (cancel) break;
                    temp[i] = (char)(temp[i] ^ a);
                    File.WriteAllText(CopyAddress, new string(temp));
                    ProgressValue = i + 1;
                    Percentage = (int)(ProgressValue * 100 / FileSize);
                    Thread.Sleep(10);
                }
            });
        }
        catch
        {
            cancel = true;
            if (ProgressValue > 0 && Percentage < 100)
            {
                File.WriteAllText(CopyAddress, defaultText);
                Percentage = 0;
                ProgressValue = 0;
            }
        }
        finally
        {
            cancel = false;
        }
    }
}