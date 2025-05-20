using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using ReactiveUI;

namespace MCSim.Console;

public class MCConsole : ReactiveObject
{

    #region Variables
    private static int maxLength = 500;
    private static string logFilePath = "./consoleLogs/MCSConsole_output.log";

    public static TextBox? textBox { get; set; }
    public static ScrollViewer? scrollViewer { get; set; }
    #endregion

    public static void Init(TextBox tb, ScrollViewer sv)
    {

        MCConsole.textBox = tb;
        MCConsole.scrollViewer = sv;
        textBox.Text = "------------------------------------\n" +
                            "Welcome to MCSim 🧫⚙ v0.1\n" +
                            "------------------------------------\n";
    }

    #region Public Methods
    public static void WriteLine(string? text = "")
    {
        Write(text + '\n');
    }

    public static void Write(string text = "")
    {
        string cleaned = RemoveSpecialSymbols(text);

        AddToTextBox(cleaned);
        WriteLog(cleaned);
    }

    public static void Clear() { if (textBox != null) textBox.Text = ""; }
    #endregion

    #region  Private Methods
    private static int NumLines()
    {
        int res = 0;
        if (textBox != null && textBox.Text != null)
        {
            for (int i = 0; i < textBox.Text.Length; i++)
            {
                char c = textBox.Text[i];
                if (c == '\n')
                {
                    res += 1;
                }
            }
            return res;
        }
        return -1;
    }

    public static string RemoveSpecialSymbols(string input)
    {
        // Regular expression to match ANSI escape codes
        string ansiEscapePattern = @"\x1B\[[0-?9;]*[mK]";
        return Regex.Replace(input, ansiEscapePattern, string.Empty);
    }

    private static void AddToTextBox(string s)
    {
        try
        {
            if (textBox != null && textBox.Text != null)
                textBox.Text += s;
            LimitLetters();
        }
        catch (Exception)
        {

            MCConsole.WriteLog($"{s}:Cant write {s} into text box.\n");

        }
    }

    private static void LimitLetters()
    {
        while (NumLines() > maxLength)
        {
            if (textBox == null || textBox.Text == null) return;
            int i = 0;
            while (textBox.Text[i] != '\n')
            {
                i++;
            }
            textBox.Text = textBox.Text.Remove(0, i + 1);
        }

        if (scrollViewer != null)
        {
            scrollViewer.ScrollToEnd();
        }
    }

    public static void WriteLog(string text)
    {
        try
        {
            File.AppendAllText(logFilePath, text);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }
    #endregion
}