using System;
using System.Collections.Generic;
using MCSim.Console;

namespace MCSim;

public class Membrane
{
    public PSystem psystem;

    public string contenido;

    public List<Tuple<string, string>> reglas;

    public Membrane(PSystem ps, string ci, List<Tuple<string, string>> r)
    {
        psystem = ps;
        contenido = ci;
        reglas = r;
    }

    public void LogState()
    {
        MCConsole.WriteLine("----------------");
        MCConsole.WriteLine("logging Membrane");
        MCConsole.WriteLine($"Contenido: {contenido}");
        MCConsole.WriteLine($"Reglas: {string.Join(", ",reglas)}");
    }
}