using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MCSim;

public class Regla
{
    [JsonIgnore]
    public Membrane membrane;
    public string input { get; set; }
    public string output { get; set; }
    public Regla(Membrane m,string input, string output)
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