using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Media;
using MCSim.Console;

namespace MCSim;

public class Membrane
{
    [JsonIgnore]

    public PSystem psystem;

    public string contenido = "";

    public int Id;

    public List<Regla> reglas;

    public Membrane? Parent;
    public List<Membrane> Children { get; set; }

    public Membrane(PSystem ps, int ki, string? ci, List<Regla>? r)
    {
        psystem = ps;
        Id = ki;
        
        if (ci != null)
            contenido = ci;
            
        if (r != null) reglas = r;
        else reglas = new List<Regla>();
        Children = new List<Membrane>();

        ps.membDict[Id] = this;
    }

    public List<Regla> GetAvaliableRules()
    {
        List<Regla> res = new List<Regla>();
        foreach (Regla r in reglas)
        {
            if (r.CanExecute())
            {
                res.Add(r);
            }
        }
        return res;
    }

    public void MakeStep()
    {
        foreach (Regla r in reglas)
        {
            if (r.CanExecute())
            {
                MCConsole.WriteLine($"{Id} - \t{r}: {r.membrane.Id}{r.CanExecute()}");
                r.Execute();
                break;
            }
        }
    }

    public void LogState()
    {
        MCConsole.WriteLine("----------------");
        PrintTree(0);
        MCConsole.WriteLine("----------------");
    }

    public void MakeChildrenFromString(string s)
    {
        Children = new List<Membrane>();
        int i = 0;

        while (i < s.Length)
        {
            if (s[i] == '[')
            {
                int start = i;
                int openBrackets = 1;
                i++;

                while (i < s.Length && openBrackets > 0)
                {
                    if (s[i] == '[') openBrackets++;
                    else if (s[i] == ']') openBrackets--;
                    i++;
                }

                if (openBrackets != 0)
                    throw new ArgumentException("Unbalanced brackets in input string.");

                int end = i;
                if (end >= s.Length)
                    throw new ArgumentException("Missing closing label after ']'.");

                string closingLabelStr = "";
                int j = end;
                while (j < s.Length && char.IsDigit(s[j]))
                {
                    closingLabelStr += s[j];
                    j++;
                }

                string fullInner = s.Substring(start + 1, end - start - 2);
                string openingLabelStr = ExtractOpeningLabel(fullInner);

                if (openingLabelStr != closingLabelStr)
                    throw new Exception($"Opening/closing label mismatch: {openingLabelStr} != {closingLabelStr}");

                int membraneKey = int.Parse(openingLabelStr);
                string innerChildren = fullInner.Substring(openingLabelStr.Length);

                var child = new Membrane(psystem, membraneKey, "", new List<Regla>()) { Parent = this};
                child.MakeChildrenFromString(innerChildren);
                Children.Add(child);

                i = j;
            }
            else
            {
                i++;
            }
        }
    }



    private string ExtractOpeningLabel(string s)
    {
        int i = 0;
        while (i < s.Length && char.IsDigit(s[i])) i++;
        return s.Substring(0, i);
    }
    
    public static string AbreviarRepeticionesGlobal(string s, int threshold = 5)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        // 1) Contar frecuencias
        var freq = new Dictionary<char,int>();
        // 2) Mantener orden de primera aparición
        var order = new List<char>();
        foreach (var c in s)
        {
            if (!freq.ContainsKey(c))
            {
                freq[c] = 0;
                order.Add(c);
            }
            freq[c]++;
        }

        // 3) Construir resultado
        var sb = new StringBuilder();
        foreach (var c in order)
        {
            int count = freq[c];
            if (count > threshold)
                sb.AppendFormat("{0}({1})\n", c, count);
            else
                sb.Append(new string(c, count));
        }

        return sb.ToString();
    }

    public void DrawTree(Panel parent, bool first = false)
    {
        WrapPanel wp = new WrapPanel();
        StackPanel sp = new StackPanel();
        sp.Children.Add(new TextBlock() { Text = Id.ToString(), Foreground = Brushes.Blue });

        foreach (Regla r in this.reglas)
        {
            string s;
            r.ValidateTIn(out s);
            if (r.ValidateTIn(out _))
            sp.Children.Add(new TextBlock()
            {
                Text = r.ToString() + $"\n{s}",
                TextWrapping = TextWrapping.Wrap,
                Foreground = r.CanExecute() ? Brushes.Green : Brushes.Red
            });
        }
        sp.Children.Add(new TextBlock() { Text = AbreviarRepeticionesGlobal(contenido), MaxWidth = 100, TextWrapping = TextWrapping.Wrap });
        wp.Children.Add(sp);
        foreach (var child in Children)
        {
            child.DrawTree(wp, false);
        }
        if (!first)
        {
            Border border = new Border()
            {
                Classes = { "card" },
                Margin = new Avalonia.Thickness(10)
            };
            border.Child = wp;
            parent.Children.Add(border);
        }
        else
        {
            parent.Children.Add(wp);
        }
    }


    public void Dissolve()
    {
        if (Parent == null) return;  // la piel no se disuelve

        // 1) Mover el contenido (sin procesar) a la membrana padre
        Parent.contenido += contenido;

        // 2) Reparentar los hijos
        foreach (var hijo in Children)
        {
            hijo.Parent = Parent;
            Parent.Children.Add(hijo);
        }

        // 3) Quitar esta membrana del padre
        Parent.Children.Remove(this);

        // 4) Eliminar del diccionario global
        psystem.membDict.Remove(this.Id);
    }


    public void PrintTree(int indent = 0)
    {
        string indentation = new string(' ', indent * 2);
        MCConsole.WriteLine($"{indentation}- Membrana: {Id} {{Content: {contenido}}}");

        foreach (var child in Children)
        {
            child.PrintTree(indent + 1);
        }
    }
}