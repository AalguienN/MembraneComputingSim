using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Avalonia.Controls;
using Avalonia.Media;
using MCSim.Console;

namespace MCSim;

public class Membrane
{
    public PSystem psystem;

    public string contenido;

    public int key;

    public List<Tuple<string, string>> reglas;

    public List<Membrane> Children { get; set; }

    public Membrane(PSystem ps, int ki, string? ci, List<Tuple<string, string>> r)
    {
        psystem = ps;
        key = ki;
        if (ci != null)
        contenido = ci;
        reglas = r;
        Children = new List<Membrane>();

        ps.membDict[key] = this;
    }

    public void LogState()
    {
        MCConsole.WriteLine("----------------");
        MCConsole.WriteLine("logging Membrane");
        MCConsole.WriteLine($"Contenido: {contenido}");
        MCConsole.WriteLine($"Reglas: {string.Join(", ", reglas)}");

        PrintTree(0);
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

                var child = new Membrane(psystem, membraneKey, "", new List<Tuple<string, string>>());
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

    public void DrawTree(Panel parent)
    {
        Border border = new Border()
        {
            Classes = { "card" },
            Margin = new Avalonia.Thickness(10)
        };
        WrapPanel wp = new WrapPanel();
        StackPanel sp = new StackPanel();
        sp.Children.Add(new TextBlock() { Text=key.ToString(), Foreground=Brushes.Red});
        sp.Children.Add(new TextBlock() { Text=contenido});
        wp.Children.Add(sp);
        foreach (var child in Children)
        {
            child.DrawTree(wp);
        }
        border.Child = wp;
        parent.Children.Add(border);
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