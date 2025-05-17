using System;
using System.Collections.Generic;

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
        
    }
}