using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MCSim.Console;
using ReactiveUI;

namespace MCSim;

public class MainWindowViewModel : ReactiveObject
{
    public ReactiveCommand<Unit, Unit> TestCommand { get; }
    public ReactiveCommand<Unit, Unit> StepCommand { get; }

    public ReactiveCommand<Unit, Unit> AddCadenaCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCadenaCommand { get; }
    public ReactiveCommand<Unit, Unit> AddReglaCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveReglaCommand { get; }

    private string _alfabeto;
    public string Alfabeto { get => _alfabeto; set => this.RaiseAndSetIfChanged(ref _alfabeto, value); }

    private string _estructuraMembranas;
    public string EstructuraMembranas { get => _estructuraMembranas; set => this.RaiseAndSetIfChanged(ref _estructuraMembranas, value); }

    private ObservableCollection<CadenaInicial> _cadenasIniciales;
    public ObservableCollection<CadenaInicial> CadenasIniciales { get => _cadenasIniciales; set => this.RaiseAndSetIfChanged(ref _cadenasIniciales, value); }

    private ObservableCollection<DefinicionRegla> _reglas;
    public ObservableCollection<DefinicionRegla> Reglas { get => _reglas; set => this.RaiseAndSetIfChanged(ref _reglas, value); }

    private int steps = 0;

    private ObservableCollection<string> _executedRules = new();
    public ObservableCollection<string> ExecutedRules
    {
        get => _executedRules;
        set => this.RaiseAndSetIfChanged(ref _executedRules, value);
    }

    public enum ExecutionMode
    {
        Secuencial,
        Máximo_Paralelo
    }
    
    public IReadOnlyList<ExecutionMode> ExecutionModes { get; } =
        Enum.GetValues<ExecutionMode>().ToList();

    private ExecutionMode _selectedMode = ExecutionMode.Secuencial;
    public ExecutionMode SelectedMode
    {
        get => _selectedMode;
        set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
    }

    public MainWindowViewModel()
    {
        TestCommand = ReactiveCommand.Create(Init);
        StepCommand = ReactiveCommand.CreateFromTask(Step);
        AddCadenaCommand = ReactiveCommand.Create(() => CadenasIniciales.Add(new CadenaInicial("", CadenasIniciales.Count)));
        RemoveCadenaCommand = ReactiveCommand.Create(() =>
        {
            if (CadenasIniciales.Any())
                CadenasIniciales.RemoveAt(CadenasIniciales.Count - 1);
        });

        AddReglaCommand = ReactiveCommand.Create(() => Reglas.Add(new DefinicionRegla(1, "", "", "", "", 1)));
        RemoveReglaCommand = ReactiveCommand.Create(() =>
        {
            if (Reglas.Any())
                Reglas.RemoveAt(Reglas.Count - 1);
        });

        _alfabeto = "abc";
        _estructuraMembranas = "[1[2]2[3[4]4]3]1";

        _cadenasIniciales = new ObservableCollection<CadenaInicial>(
            new[] { "ab", "aaab", "bbbbccc", "cbbb" }
            .Select((val, i) => new CadenaInicial(val, i))
        );

        _reglas = new ObservableCollection<DefinicionRegla>()
        {
            new DefinicionRegla(1,"a","aa","2:a;","a",1),
            new DefinicionRegla(2,"b","bb","","",1),
            new DefinicionRegla(2,"bb","b","","",1),
            new DefinicionRegla(3,"c","a","","",1),
            new DefinicionRegla(4,"b","a","","",1),
        };
    }

    private PSystem? _pSystem;
    public PSystem? PSystem { get => _pSystem; set => this.RaiseAndSetIfChanged(ref _pSystem, value); }

    public void Init()
    {
        List<char> _alf = Alfabeto.ToCharArray().ToList();
        string _estrMem = EstructuraMembranas;
        List<string> _cadIni = CadenasIniciales.Select(ci => (string)ci).ToList();
        List<Tuple<int, string, string, string, string, int>> _reg = Reglas.Select(ri => Tuple.Create(ri.M, ri.Input, ri.T_Here,ri.T_in,ri.T_out, ri.Priority)).ToList();
        PSystem = new PSystem(alfabeto: _alf,
                            estructuraMembranas: _estrMem,
                            cadenasIniciales: _cadIni,
                            reglas: _reg,
                            prioridades: new List<Tuple<int, int>>()
                        );

        if (MainWindow.Instance != null)
        {
            MainWindow.Instance.CellsPanel.Children.Clear();
            PSystem.rootMembranas.DrawTree(MainWindow.Instance.CellsPanel, first: true);
        }
    }

    private bool executing;

    public async Task Step()
    {
        if (executing) return;

        executing = true;
        if (PSystem == null) return;

        steps++;

        await Task.Run(() =>
        {
            if (SelectedMode == ExecutionMode.Secuencial)
                PSystem.MakeStep();
            else
                PSystem.MakeParallelStep();
        });


        // ExecutedRules.Clear();
        ExecutedRules.Add($"Step: {steps}");
        foreach (var txt in PSystem.LastExecutedRules)
            ExecutedRules.Add(txt);

        MainWindow.Instance?.ScrollRules.ScrollToEnd();
        // ExecutedRules.Add("\n");


        if (MainWindow.Instance != null && PSystem != null)
        {
            MainWindow.Instance.CellsPanel.Children.Clear();
            PSystem.rootMembranas.DrawTree(MainWindow.Instance.CellsPanel, first: true);
        }
        executing = false;
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
        this.index = index+1;
    }

    public static implicit operator CadenaInicial(string s) => new CadenaInicial(s);

    public static implicit operator string(CadenaInicial ci) => ci.Valor;
}
