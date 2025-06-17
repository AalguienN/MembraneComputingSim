using System;
using Avalonia.Controls;
using MCSim.Console;

namespace MCSim;

public partial class MainWindow : Window
{
    public static MainWindow? Instance;
    private MainWindowViewModel viewModel;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = new MainWindowViewModel();
        DataContext = viewModel;
        MCConsole.Init(tb: MCSConsoleTextBox);
        Instance = this;
    }
}