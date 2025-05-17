using System.Reactive;
using MCSim.Console;
using ReactiveUI;

namespace MCSim;

public class MainWindowViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> TestCommand { get; }

    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.Create(Test);
    }
    
    public void Test()
    {
        MCSConsole.WriteLine("TEST......................................................................................................................................................................................................................................................................................");
        MCSConsole.WriteLine("TEST..........................................................");
        MCSConsole.WriteLine("TEST..........................................................");
        MCSConsole.WriteLine("TEST..........................................................");
    }
}