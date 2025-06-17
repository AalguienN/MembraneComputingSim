using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCSim.Console;

namespace MCSim;

public class PSystem
{
    //alfabeto
    public List<char> alfabeto;

    //Estructura membranas
    public Membrane rootMembranas;

    public Dictionary<int, Membrane> membDict;
    //Falta definir las membranas

    private static readonly Random _rng = new Random();

    public List<string> LastExecutedRules { get; private set; } = new();

    //Cadenas iniciales en cada membrana

    public PSystem(
        List<char> alfabeto,
        string estructuraMembranas,
        List<string> cadenasIniciales,
        List<Tuple<int, string, string, string, string, int>> reglas,  // (region, antecedente, consecuente)
        List<Tuple<int, int>> prioridades          // (reglaMayor, reglaMenor)
    )
    {
        MCConsole.WriteLine("\n Incializando PSystem:");

        MCConsole.WriteLine($" · Alfabeto: {string.Join(", ", alfabeto)}");

        MCConsole.WriteLine($" · Estructura: {estructuraMembranas}");

        MCConsole.WriteLine($" · Cadenas iniciales: {string.Join(", ", cadenasIniciales)}");

        MCConsole.WriteLine($" · Reglas: {string.Join(" ", reglas)}");

        MCConsole.WriteLine($" · Prioridades: {string.Join(", ", cadenasIniciales)}");
        membDict = new Dictionary<int, Membrane>();

        this.alfabeto = alfabeto;
        rootMembranas = new Membrane(
            ps: this,
            ki: 0,
            ci: null,
            r: null
        );

        rootMembranas.MakeChildrenFromString(estructuraMembranas);
        SetContentAll(cadenasIniciales);
        SetRulesAll(reglas);
        rootMembranas.LogState();
    }

    private void SetContentAll(List<string> Contenido)
    {
        for (int k = 0; k < Contenido.Count; k++)
        {
            if (membDict.Keys.Contains(k + 1))
            {
                membDict[k + 1].contenido = Contenido[k];
            }
            else
            {
                MCConsole.WriteLine($"{k} not in keys: {membDict.Keys.Count}");
            }
        }
    }

    private void SetRulesAll(List<Tuple<int, string, string, string, string, int>> reglas)
    {
        List<Regla> _reglas = new List<Regla>();
        foreach ((int k, string i, string _h, string _i, string _o, int _priority) in reglas)
        {
            if (i == "" || (_h == "" && _i == "" && _o == "")) continue;
            if (membDict.Keys.Contains(k))
            {
                Regla reg = new Regla(membDict[k], i, _h, _i, _o, priority: _priority);
                membDict[k].reglas.Add(reg);
                _reglas.Add(reg);
            }
            else
            {
                MCConsole.WriteLine($"{k} not in keys: {membDict.Keys.Count}");
            }
        }

        foreach (Regla r in _reglas)
        {
            string errorMessage;
            r.ValidateTIn(out errorMessage);
        }
    }

    public List<Regla> GetAvaliableRules()
    {
        List<Regla> res = new List<Regla>();
        foreach (int m in membDict.Keys)
        {
            res.AddRange(membDict[m].GetAvaliableRules());
        }
        return res;
    }

    public void MakeStep()
    {
        var available = GetAvaliableRules();
        if (available.Count == 0)
            return;

        int idx = _rng.Next(available.Count);
        var r = available[idx];

        LastExecutedRules.Clear();

        LastExecutedRules.Add(r.ToString());

        r.Execute();
    }
    
    public static Dictionary<char,int> BuildMultiset(string s)
    {
        var mult = new Dictionary<char,int>();
        foreach(var c in s)
        {
            if (mult.ContainsKey(c)) mult[c]++;
            else mult[c] = 1;
        }
        return mult;
    }

    public static int GetLevel(Regla r)
    {
        return r.Priority;
    }

    public void MakeParallelStep()
    {
        // Paso 1: calcular k_r para cada regla
        // guardamos para cada regla su multiplicidad
        var mults = new Dictionary<Regla, int>();

        foreach (var mem in membDict.Values)
        {
            // 1a. si usas prioridades, selecciona el primer nivel aplicable
            var byLevel = mem.reglas
                            .GroupBy(r => GetLevel(r))    // tu función que devuelve la prioridad numérica
                            .OrderByDescending(g => g.Key)
                            .ToList();
            List<Regla> toApply;
            if (byLevel.Count > 0)
            {
                toApply = new List<Regla>();
                foreach (var group in byLevel)
                {
                    if (group.Any(r => r.CanExecute(mem.contenido)))
                    {
                        toApply = group.ToList();
                        break;
                    }
                }

            }
            else
            {
                toApply = new List<Regla>();
            }

            // 1b. para cada regla r en toApply, computa k_r = máxima repeticiones
            //    basándote en cuántas veces cabe input(r) en contenido
            var avail = BuildMultiset(mem.contenido);
            foreach (var r in toApply)
            {
                int k = int.MaxValue;
                foreach (var c in r.input.GroupBy(ch => ch))
                {
                    char simbolo = c.Key;
                    int req = c.Count();
                    int have = avail.TryGetValue(simbolo, out var cnt) ? cnt : 0;
                    k = Math.Min(k, have / req);
                }
                if (k > 0) mults[r] = k;
            }
        }

        // Preparamos estructuras para añadir/quitar
        var removeCounts = new Dictionary<Membrane, Dictionary<char, int>>();
        var addHere = new Dictionary<Membrane, List<char>>();
        var addOut = new Dictionary<Membrane, List<char>>();
        var addIn = new Dictionary<Membrane, List<(Membrane target, string str)>>();

        foreach (var mem in membDict.Values)
        {
            removeCounts[mem] = new Dictionary<char, int>();
            addHere[mem] = new List<char>();
            addOut[mem] = new List<char>();
            addIn[mem] = new List<(Membrane, string)>();
        }

        LastExecutedRules.Clear();
        // Paso 2 y 3: distribuir efectos según k_r
        foreach (var kv in mults)
        {
            var r = kv.Key;
            int k = kv.Value;

            if (k > 0)
                LastExecutedRules.Add(r.ToString() + $"(x{k})");

            var mem = r.membrane;

            // 2) acumula las restas
            foreach (char c in r.input)
                removeCounts[mem][c] = removeCounts[mem].GetValueOrDefault(c) + k;

            // 3a) aquí
            for (int i = 0; i < k; i++)
                addHere[mem].AddRange(r.t_here);

            // 3b) out
            if (mem.Parent != null)
                for (int i = 0; i < k; i++)
                    addOut[mem.Parent].AddRange(r.t_out);

            // 3c) in: parsea r.t_in "id:cad;..."
            for (int i = 0; i < k; i++)
            {
                foreach (var segmento in r.t_in.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var partes = segmento.Split(':', 2);
                    if (partes.Length != 2) continue;
                    if (int.TryParse(partes[0], out var targetId))
                    {
                        var hija = mem.Children.FirstOrDefault(ch => ch.Id == targetId);
                        if (hija != null)
                            addIn[hija].Add((hija, partes[1]));
                    }
                }
            }
        }

        // Paso 4: aplicar quitas y puestas a cada membrana
        foreach (var mem in membDict.Values)
        {
            // 4a) quitar inputs
            var content = new List<char>(mem.contenido);
            foreach (var kv in removeCounts[mem])
            {
                for (int i = 0; i < kv.Value; i++)
                    content.Remove(kv.Key);
            }

            // 4b) añadir t_here
            content.AddRange(addHere[mem]);

            mem.contenido = new string(content.ToArray());

            // 4c) añadir salidas al padre
            if (addOut.TryGetValue(mem, out var outs) && outs.Count > 0)
                mem.contenido += new string(outs.ToArray());

            // 4d) añadir entradas de hijas
            if (addIn.TryGetValue(mem, out var ins) && ins.Count > 0)
                mem.contenido += string.Concat(ins.Select(t => t.str));
        }
    }

}