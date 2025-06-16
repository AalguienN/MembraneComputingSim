using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MCSim.Console;
using ReactiveUI;

namespace MCSim;

public class MainWindowViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> TestCommand { get; }
    public ReactiveCommand<Unit, Unit> StepCommand { get; }

    public ReactiveCommand<Unit, Unit> AddCadenaCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCadenaCommand { get; }

    private string _alfabeto;
    public string Alfabeto { get => _alfabeto; set => this.RaiseAndSetIfChanged(ref _alfabeto, value); }

    private string _estructuraMembranas;
    public string EstructuraMembranas { get => _estructuraMembranas; set => this.RaiseAndSetIfChanged(ref _estructuraMembranas, value); }

    private ObservableCollection<CadenaInicial> _cadenasIniciales;
    public ObservableCollection<CadenaInicial> CadenasIniciales { get => _cadenasIniciales; set => this.RaiseAndSetIfChanged(ref _cadenasIniciales, value); }

    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.Create(Init);
        StepCommand = ReactiveCommand.Create(Step);
        AddCadenaCommand = ReactiveCommand.Create(() => CadenasIniciales.Add(new CadenaInicial("", CadenasIniciales.Count)));
        RemoveCadenaCommand = ReactiveCommand.Create(() =>
        {
            if (CadenasIniciales.Any())
                CadenasIniciales.RemoveAt(CadenasIniciales.Count - 1);
        });

        _alfabeto = "abc";
        _estructuraMembranas = "[1[2]2[3[4]4]3]1";

        _cadenasIniciales = new ObservableCollection<CadenaInicial>(
            new[] { "ab", "aaab", "bbbbccc", "cbbb" }
            .Select((val, i) => new CadenaInicial(val, i))
        );


    }

    private PSystem? _pSystem;
    public PSystem? PSystem { get => _pSystem; set => this.RaiseAndSetIfChanged(ref _pSystem, value); }

    public void Init()
    {

        List<char> _alf = Alfabeto.ToCharArray().ToList();
        string _estrMem = EstructuraMembranas;
        List<string> _cadIni = CadenasIniciales.Select(ci => (string)ci).ToList();

        PSystem = new PSystem(alfabeto: _alf,
                            estructuraMembranas: _estrMem,
                            cadenasIniciales: _cadIni,
                            reglas: new List<System.Tuple<int, string, string>> {
                                    new Tuple<int,string,string>(1,"a","aa"),
                                    new Tuple<int,string,string>(2,"b","ab"),
                                    new Tuple<int,string,string>(3,"c","a"),
                                    new Tuple<int,string,string>(4,"b","a"),
                                },
                            prioridades: new List<Tuple<int, int>>()
                        );

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.CellsPanel.Children.Clear();
            PSystem.rootMembranas.DrawTree(MainWindow.Instance.CellsPanel, first: true);
        }
    }

    public void Step()
    {
        PSystem?.MakeStep();
        if (MainWindow.Instance != null && PSystem != null)
        {
            MainWindow.Instance.CellsPanel.Children.Clear();
            PSystem.rootMembranas.DrawTree(MainWindow.Instance.CellsPanel, first: true);
        }
    }
}


public class CadenaInicial : ReactiveObject
{
    public int index { get; set; }
    private string _valor;
    public string Valor
    {
        get => _valor;
        set => this.RaiseAndSetIfChanged(ref _valor, value);
    }

    public CadenaInicial(string valor = "", int index = 0)
    {
        this._valor = valor;
        this.index = index;
    }

    // Conversión implícita: string → CadenaInicial
    public static implicit operator CadenaInicial(string s) => new CadenaInicial(s);

    // Conversión implícita: CadenaInicial → string
    public static implicit operator string(CadenaInicial ci) => ci.Valor;
}
