using System;
using System.Collections.Generic;
using System.Data;
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
        List<Tuple<int, string, string, string, string, int, bool>> reglas,  // (region, antecedente, consecuente)
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

    private void SetRulesAll(List<Tuple<int, string, string, string, string, int, bool>> reglas)
    {
        List<Regla> _reglas = new List<Regla>();
        foreach ((int k, string i, string _h, string _i, string _o, int _priority, bool _isDissolution) in reglas)
        {
            if (i == "" || (_h == "" && _i == "" && _o == "")) continue;
            if (membDict.Keys.Contains(k))
            {
                Regla reg = new Regla(membDict[k], i, _h, _i, _o, priority: _priority, isDissolution: _isDissolution);
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
        LastExecutedRules.Clear();
        LastExecutedRules.Add("∅");
        var available = GetAvaliableRules();
        if (available.Count == 0)
            return;
        LastExecutedRules.Clear();

        int idx = _rng.Next(available.Count);
        var r = available[idx];


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
        // Paso 1: calcular cuántas veces aplicar cada regla y cuáles membranas disolver
        var mults = new Dictionary<Regla,int>();
        var toDissolve = new List<Membrane>();

        foreach (var mem in membDict.Values.ToList())
        {
            // agrupamos reglas de esta membrana por nivel de prioridad
            var byLevel = mem.reglas
                            .GroupBy(r => GetLevel(r))
                            .OrderByDescending(g => g.Key);

            // elegimos el primer nivel que tenga alguna regla aplicable
            var topGroup = byLevel.FirstOrDefault(g => g.Any(r => r.CanExecute(mem.contenido)));
            if (topGroup == null)
                continue;

            // construimos el mults para este nivel
            var avail = BuildMultiset(mem.contenido);
            foreach (var r in topGroup)
            {
                if (!r.CanExecute(mem.contenido))
                    continue;

                if (r.IsDissolution)
                {
                    // disolución: sólo una vez
                    mults[r] = 1;
                    toDissolve.Add(mem);
                }
                else
                {
                    // calcular cuántas veces cabe input(r)
                    int k = int.MaxValue;
                    foreach (var grp in r.input.GroupBy(c => c))
                    {
                        int have = avail.TryGetValue(grp.Key, out var cnt) ? cnt : 0;
                        k = Math.Min(k, have / grp.Count());
                    }
                    if (k > 0)
                        mults[r] = k;
                }
            }
        }

        // preparar estructuras para el efecto de las reglas no-disolución
        var removeCounts = membDict.Values.ToDictionary(
            m => m,
            m => new Dictionary<char,int>());

        var addHere = membDict.Values.ToDictionary(m => m, m => new List<char>());
        var addOut  = membDict.Values.ToDictionary(m => m, m => new List<char>());
        var addIn   = membDict.Values.ToDictionary(m => m, m => new List<(Membrane,string)>());

        LastExecutedRules.Clear();

        // Paso 2 & 3: aplicar efectos de todas las reglas (excepto disolución)
        foreach (var kv in mults)
        {
            var r = kv.Key;
            int k = kv.Value;
            var mem = r.membrane;

            if (r.IsDissolution)
            {
                LastExecutedRules.Add($"δ@{mem.Id}");
                continue;
            }

            // log de la regla
            LastExecutedRules.Add(r.ToString() + (k > 1 ? $"(x{k})" : ""));

            // 2) contabilizar consumo de input
            foreach (char c in r.input)
                removeCounts[mem][c] = removeCounts[mem].GetValueOrDefault(c) + k;

            // 3a) añadir t_here k veces
            for (int i = 0; i < k; i++)
                addHere[mem].AddRange(r.t_here);

            // 3b) output al padre k veces
            if (mem.Parent != null && !string.IsNullOrEmpty(r.t_out))
                for (int i = 0; i < k; i++)
                    addOut[mem.Parent].AddRange(r.t_out);

            // 3c) distribuir t_in k veces
            for (int i = 0; i < k; i++)
            {
                if (string.IsNullOrWhiteSpace(r.t_in))
                    break;

                foreach (var seg in r.t_in.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var partes = seg.Split(':', 2);
                    if (partes.Length != 2) continue;
                    if (!int.TryParse(partes[0], out var targetId)) continue;
                    var hija = mem.Children.FirstOrDefault(ch => ch.Id == targetId);
                    if (hija != null)
                        addIn[hija].Add((hija, partes[1]));
                }
            }
        }

        // Paso 4: actualizar contenidos de cada membrana tras inputs/outputs y producción local
        foreach (var mem in membDict.Values.ToList())
        {
            // 4a) quitar inputs
            var content = new List<char>(mem.contenido);
            foreach (var kv in removeCounts[mem])
                for (int i = 0; i < kv.Value; i++)
                    content.Remove(kv.Key);

            // 4b) añadir t_here
            content.AddRange(addHere[mem]);
            mem.contenido = new string(content.ToArray());

            // 4c) añadir outputs al padre
            if (addOut[mem].Count > 0)
                mem.contenido = mem.contenido + new string(addOut[mem].ToArray());

            // 4d) añadir inputs desde hijas
            if (addIn[mem].Count > 0)
                mem.contenido += string.Concat(addIn[mem].Select(t => t.Item2));
        }

        // Paso 5: finalmente ejecutar las disoluciones
        foreach (var mem in toDissolve.ToList())
        {
            // la disolución mueve contenido e hijos al padre y elimina la membrana
            mem.Dissolve();
        }
    }


}