using System;
using Avalonia.Controls;
using MCSim.Console;

namespace MCSim;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        MCConsole.Init(tb: MCSConsoleTextBox, sv: MCSConsoleScroller);
    }
}