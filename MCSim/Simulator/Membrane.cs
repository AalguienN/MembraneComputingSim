using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using Avalonia.Media;
using MCSim.Console;

namespace MCSim;

public class Membrane
{
    [JsonIgnore]
    public PSystem psystem;

    public string contenido;

    public int key;

    public List<Regla> reglas;

    public List<Membrane> Children { get; set; }

    public Membrane(PSystem ps, int ki, string? ci, List<Regla>? r)
    {
        psystem = ps;
        key = ki;
        
        if (ci != null)
            contenido = ci;
            
        if (r != null) reglas = r;
        else reglas = new List<Regla>();
        Children = new List<Membrane>();

        ps.membDict[key] = this;
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
                MCConsole.WriteLine($"{key} - \t{r}: {r.membrane.key}{r.CanExecute()}");
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

                var child = new Membrane(psystem, membraneKey, "", new List<Regla>());
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

    public void DrawTree(Panel parent, bool first = false)
    {
        WrapPanel wp = new WrapPanel();
        StackPanel sp = new StackPanel();
        sp.Children.Add(new TextBlock() { Text = key.ToString(), Foreground = Brushes.Blue });
        foreach (Regla r in this.reglas)
        {
            sp.Children.Add(new TextBlock()
            {
                Text = r.ToString(),
                TextWrapping = TextWrapping.Wrap,
                Foreground = r.CanExecute() ? Brushes.Green : Brushes.Red
            });
        }
        sp.Children.Add(new TextBlock() { Text = contenido, MaxWidth=100, TextWrapping=TextWrapping.Wrap });
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


    public void PrintTree(int indent = 0)
    {
        string indentation = new string(' ', indent * 2);
        MCConsole.WriteLine($"{indentation}- Membrana: {key} {{Content: {contenido}}}");

        foreach (var child in Children)
        {
            child.PrintTree(indent + 1);
        }
    }
}