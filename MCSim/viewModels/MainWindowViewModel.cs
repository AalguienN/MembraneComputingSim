using System;
using System.Collections.Generic;
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
        _ = new PSystem(alfabeto: new List<char> { 'a', 'b', 'c' },
                            estructuraMembranas: "[1[2][3[4]]]",
                            cadenasIniciales: new List<string> { "ab", "aaa", "bbbb", "c" },
                            reglas: new List<System.Tuple<int, string, string>> {
                                    new Tuple<int,string,string>(1,"",""),
                                    new Tuple<int,string,string>(2,"",""),
                                    new Tuple<int,string,string>(3,"",""),
                                    new Tuple<int,string,string>(4,"",""),
                                },
                            prioridades: new List<Tuple<int, int>>()
                        );
    }
}