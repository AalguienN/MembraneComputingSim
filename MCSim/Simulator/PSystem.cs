using System;
using System.Collections.Generic;
using System.Linq;
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

    //Cadenas iniciales en cada membrana

    public PSystem(
        List<char> alfabeto,
        string estructuraMembranas,
        List<string> cadenasIniciales,
        List<Tuple<int, string, string>> reglas,  // (region, antecedente, consecuente)
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
            if (membDict.Keys.Contains(k+1))
            {
                membDict[k+1].contenido = Contenido[k];
            }
            else
            {
                MCConsole.WriteLine($"{k} not in keys: {membDict.Keys.Count}");
            }
        }
    }

    private void SetRulesAll(List<Tuple<int, string, string>> reglas)
    {
        foreach ((int k, string i, string o) in reglas)
        {
            if (membDict.Keys.Contains(k))
            {
                membDict[k].reglas.Add(new Regla(membDict[k], i, o));
            }
            else
            { 
                MCConsole.WriteLine($"{k} not in keys: {membDict.Keys.Count}");
            }

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
        List<Regla> reglas = GetAvaliableRules();
        int idx = _rng.Next(reglas.Count);
        Regla r = reglas[idx];
        r.Execute();
    }
}