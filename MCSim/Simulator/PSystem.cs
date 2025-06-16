using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Metadata;
using MCSim.Console;
using MCSim.Utils;

namespace MCSim;

public class PSystem
{
    //alfabeto
    public List<char> alfabeto;

    //Estructura membranas
    public Membrane rootMembranas;

    public Dictionary<int, Membrane> membDict;
    //Falta definir las membranas

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
            r: new List<Tuple<string, string>>
            {
                new Tuple<string,string>("","")
            }
        );

        rootMembranas.MakeChildrenFromString(estructuraMembranas);
        SetContentAll(cadenasIniciales);
        rootMembranas.LogState();
    }

    private void SetContentAll(List<string> Contenido)
    {
        foreach (int k in membDict.Keys.Order())
        {
            if (k == 0) continue;
            if (membDict.Keys.Contains(k))
            {
                membDict[k].contenido = Contenido[k - 1];
            }
            else
            {
                MCConsole.WriteLine($"{k} not in keys: {membDict.Keys.Count}");
            }
        }
    }
}