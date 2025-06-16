using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MCSim;
using ReactiveUI;

public class Regla
{
    [JsonIgnore]
    public Membrane membrane;
    public string input { get; set; }
    public string output { get; set; }
    public Regla(Membrane m, string input, string output)
    {
        this.membrane = m;
        this.input = input;
        this.output = output;
    }

    public override string ToString()
    {
        return $"{input}->{output}";
    }

    public bool CanExecute()
    {
        return CanExecute(membrane.contenido);
    }

    public bool CanExecute(string s)
    {
        var availiable = new Dictionary<char, int>();
        foreach (char c in s)
        {
            if (availiable.ContainsKey(c))
                availiable[c]++;
            else availiable[c] = 1;
        }
        foreach (char c in input)
        {
            if (!availiable.TryGetValue(c, out int cnt) || cnt == 0)
            {
                return false;
            }
            availiable[c] = cnt - 1;
        }
        return true;
    }

    public void Execute()
    {
        if (!CanExecute())
            throw new InvalidOperationException(
                $"La regla {this} no es aplicable a \"{membrane.contenido}\"");

        var lista = new List<char>(membrane.contenido);
        foreach (char c in input)
        {
            lista.Remove(c);
        }

        lista.AddRange(output);

        membrane.contenido = new string(lista.ToArray());
    }
}

public class DefinicionRegla : ReactiveObject
{
    private int _m;
    public int M { get => _m; set => this.RaiseAndSetIfChanged(ref _m, value); }
    private string _input;
    public string Input { get => _input; set => this.RaiseAndSetIfChanged(ref _input, value); }
    private string _output;
    public string Output { get => _output; set => this.RaiseAndSetIfChanged(ref _output, value); }
    public DefinicionRegla(int m, string input, string output)
    {
        this._m = m;
        this._input = input;
        this._output = output;
    }

    // Conversión implícita: Tuple → DefinicionRegla
    public static implicit operator DefinicionRegla((int n, string i, string o) tuple)
        => new DefinicionRegla(tuple.n, tuple.i, tuple.o);

    // Conversión implícita: CadenaInicial → string
    public static implicit operator Tuple<int,string,string>(DefinicionRegla d) => new Tuple<int, string, string>(d.M,d.Input,d.Output);
}