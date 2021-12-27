using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisableTaskbar;

public partial class MainWindow : Form
{
    [DllImport("user32.dll")]
    private static extern int FindWindow(string className, string windowText);
    [DllImport("user32.dll")]
    private static extern int ShowWindow(int hwnd, int command);
    // DLL libraries used to manage hotkeys
    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    private static readonly string StartupKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
    private static readonly string StartupValue = "HarkiratSinghTaskbarHideApp";


    private const int HANDLE_WIN_HOTKEY = 100;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 1;

    bool isVisible = true;

    const string TaskbarWindow = "Shell_TrayWnd";

    private static void SetStartup(bool val)
    {
        //Set the application to run at startup
        RegistryKey key = Registry.CurrentUser.OpenSubKey(StartupKey, true);
        if (val)
            key!.SetValue(StartupValue, Application.ExecutablePath.ToString());
        else
            key!.DeleteValue(StartupValue);
    }

    
    private bool AutoStartUp
    {
        get => (bool)Properties.Settings.Default["startEnabled"];
        set
        {
            Properties.Settings.Default["startEnabled"] = value;
            Properties.Settings.Default.Save();
        }
    }
    private char KeyValue
    {
        get
        {
            try
            {
                return (char)Properties.Settings.Default["index"];
            }
            catch
            {
                Properties.Settings.Default["index"] = 'J';
                Properties.Settings.Default.Save();
                return (char)Properties.Settings.Default["index"];
            }
        }
        set
        {
            Properties.Settings.Default["index"] = value;
            Properties.Settings.Default.Save();
        }
    }
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ToggleTaskbar()
    {
        int hwnd = FindWindow(TaskbarWindow, string.Empty);
        ShowWindow(hwnd, isVisible ? SW_HIDE : SW_SHOW);
        isVisible = !isVisible;
        
    }

    private void disableBtn_Click(object sender, EventArgs e)
    {
        ToggleTaskbar();
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        notifyIcon.Icon = this.Icon;

        DataTable dt = new DataTable();
        dt.Columns.Add(new DataColumn("name", typeof(string)));
        dt.Columns.Add(new DataColumn("value", typeof(int)));
        // Add Alphabet Keys
        for (char i = 'A'; i <= 'Z'; i++)
        {
            dt.Rows.Add(i.ToString(), (int)i);
        }

        comboBox3.DataSource = dt;
        comboBox3.DisplayMember = "name";
        comboBox3.ValueMember = "value";
        comboBox3.SelectedIndex = KeyValue - 'A';
        if (!RegisterHotKey(this.Handle, HANDLE_WIN_HOTKEY, 0x8 | 0x4, KeyValue))
        {
            MessageBox.Show("Unable to set up hotkeys. Choose another key");
            comboBox3.SelectedIndex = -1;
        }

        startCheck.Checked = AutoStartUp;
        if (AutoStartUp)
        {
            this.WindowState = FormWindowState.Minimized;
            //this.Hide();
            //notifyIcon.Visible = true;
        }
    }

    private void exitOption_Click(object sender, EventArgs e)
    {
        int hwnd = FindWindow(TaskbarWindow, string.Empty);
        ShowWindow(hwnd, SW_SHOW);
        Application.Exit();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x0312 && m.WParam.ToInt32() == HANDLE_WIN_HOTKEY)
        {
            ToggleTaskbar();
        }
        base.WndProc(ref m);
    }

    private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        UnregisterHotKey(this.Handle, HANDLE_WIN_HOTKEY);
    }

    private void registerBtn_Click(object sender, EventArgs e)
    {
        if (comboBox3.SelectedItem == null)
            MessageBox.Show("Select a key");
        UnregisterHotKey(this.Handle, HANDLE_WIN_HOTKEY);
        int prev = KeyValue;
        KeyValue = Convert.ToChar(comboBox3.SelectedValue);
        if (!RegisterHotKey(this.Handle, HANDLE_WIN_HOTKEY, 0x8 | 0x4, KeyValue))
        {
            MessageBox.Show("Unable to set up hotkeys. Choose another key");
            comboBox3.SelectedIndex = prev - 'A';
            KeyValue = Convert.ToChar(prev);
        }
    }

    private void MainWindow_Resize(object sender, EventArgs e)
    {
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide();
            notifyIcon.Visible = true;
        }
    }

    private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
    }

    private void ensureShow_Click(object sender, EventArgs e)
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        AutoStartUp = startCheck.Checked;
        SetStartup(AutoStartUp);
    }
}
