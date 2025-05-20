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
    public List<Membrane> membranas;
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

        Membrane m = new Membrane(
            ps: this,
            ci: cadenasIniciales[0],
            r: new List<Tuple<string, string>>
            {
                new Tuple<string,string>("","")
            }
        );

        m.LogState();
    }
}