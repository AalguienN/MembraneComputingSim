using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MCSim;
using ReactiveUI;

public class Regla
{
    [JsonIgnore]
    public Membrane membrane;
    public string input { get; set; }
    public string t_here { get; set; }
    public string t_in { get; set; }
    public string t_out { get; set; }
    public int Priority {get;set;}
    public Regla(Membrane m, string input, string t_here = "", string t_in = "", string t_out = "", int priority = 1)
    {
        this.membrane = m;
        this.input = input;
        this.t_here = t_here;
        this.t_in = t_in;
        this.t_out = t_out;
        this.Priority = priority;
    }

    public override string ToString()
    {
        string tin = t_in != "" ? $"({t_in})in" : "";
        string tout = t_out != "" ? $"({t_out})out" : "";
        return $"{input}->{t_here} {tin} {tout}";
    }

    public bool CanExecute()
    {
        return CanExecute(membrane.contenido);
    }

    public bool CanExecute(string s)
    {
        if (input == "" || (t_here == "" && t_in == "" && t_out == "")) return false;
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
        return true && ValidateTIn(out _);
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

        if (!string.IsNullOrEmpty(t_out) && membrane.Parent != null)
        {
            membrane.Parent.contenido += t_out;
        }

        // Distribuye t_in entre las hijas según el mapeo "id:cadena;id2:cadena2"
        if (!string.IsNullOrWhiteSpace(t_in))
        {
            // Para cada segmento "id:subcadena"
            foreach (var segmento in t_in.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                if (segmento == "") continue;
                var partes = segmento.Split(':', 2);
                if (partes.Length != 2) continue; // o lanza excepción
                if (int.TryParse(partes[0], out int targetId))
                {
                    var contenidoAEnviar = partes[1];
                    // Busca la membrana hija con ese Id
                    var hija = membrane.Children.FirstOrDefault(ch => ch.Id == targetId);
                    if (hija != null)
                    {
                        hija.contenido += contenidoAEnviar;
                    }
                    else
                    {
                        // opcional: log o excepción si el Id no existe
                    }
                }
            }
        }

        lista.AddRange(t_here);

        membrane.contenido = new string(lista.ToArray());
    }
    

    public bool ValidateTIn(out string errorMessage)
    {
        errorMessage = "";
        if (string.IsNullOrWhiteSpace(t_in))
            return true;

        // Recorremos cada segmento "id:cadena"
        foreach (var segmento in t_in.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = segmento.Split(':', 2, StringSplitOptions.None);
            if (partes.Length != 2)
            {
                errorMessage = $"Segmento inválido «{segmento}». Debe tener formato id:cadena.";
                return false;
            }

            // Validamos que el id sea un entero
            if (!int.TryParse(partes[0], out int targetId))
            {
                errorMessage = $"ID de membrana no numérico en segmento «{segmento}».";
                return false;
            }

            // Validamos que exista la hija con ese Id
            bool existe = membrane.Children.Any(ch => ch.Id == targetId);
            if (!existe)
            {
                errorMessage = $"No existe membrana hija con Id={targetId} para segmento «{segmento}».";
                return false;
            }
        }

        return true;
    }

}

public class DefinicionRegla : ReactiveObject
{
    private int _m;
    public int M { get => _m; set => this.RaiseAndSetIfChanged(ref _m, value); }
    private string _input;
    public string Input { get => _input; set => this.RaiseAndSetIfChanged(ref _input, value); }
    private string _t_here;
    public string T_Here { get => _t_here; set => this.RaiseAndSetIfChanged(ref _t_here, value); }
    private string _t_in;
    public string T_in { get => _t_in; set => this.RaiseAndSetIfChanged(ref _t_in, value); }
    private string _t_out;
    public string T_out { get => _t_out; set => this.RaiseAndSetIfChanged(ref _t_out, value); }
    private int _priority;
    public int Priority { get => _priority; set => this.RaiseAndSetIfChanged(ref _priority, value); }

    public DefinicionRegla(int m, string input, string t_here, string t_in, string t_out, int priority)
    {
        this._m = m;
        this._input = input;
        this._t_here = t_here;
        this._t_in = t_in;
        this._t_out = t_out;
        this._priority = priority;
    }

    // Conversión implícita: Tuple → DefinicionRegla
    public static implicit operator DefinicionRegla((int n, string i, string _h, string _i, string _o, int _priority) tuple)
        => new DefinicionRegla(tuple.n, tuple.i, tuple._h, tuple._i, tuple._o, tuple._priority);

    // Conversión implícita: CadenaInicial → string
    public static implicit operator Tuple<int,string,string,string,string,int>(DefinicionRegla d) => new Tuple<int, string, string, string, string, int>(d.M,d.Input,d.T_Here,d.T_in,d.T_out,d._priority);
}