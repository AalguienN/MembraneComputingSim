using System;
using Avalonia.Controls;
using MCSim.MCConsole;

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
        MCConsole.MCConsole.Init(tb: MCSConsoleTextBox);
        Instance = this;
    }
}