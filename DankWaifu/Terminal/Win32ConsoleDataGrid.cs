#if !MONO

using System;
using System.Collections.Generic;
using System.Text;

namespace DankWaifu.Terminal
{
    public class Win32ConsoleDataGrid
    {
        private static readonly IntPtr StdOutPtr;

        static Win32ConsoleDataGrid()
        {
            StdOutPtr = Win32ConsoleNativeMethods.GetStdHandle(-11);
        }

        private readonly int _lines;
        private readonly int _columns;

        private readonly List<string[]> _items;

        public Win32ConsoleDataGrid(int lines, int columns)
        {
            _lines = lines;
            _columns = columns;

            if (lines > Console.BufferHeight)
                Console.SetBufferSize(Console.BufferWidth, lines);

            _items = new List<string[]>();
            for (var i = 0; i < lines; i++)
                _items.Add(new string[columns]);
        }

        public void UpdateItem(int line, int column, string val)
        {
            if (line >= _lines)
                throw new ArgumentOutOfRangeException(nameof(line));

            if (column >= _columns)
                throw new ArgumentOutOfRangeException(nameof(column));

            var prevLineLen = 0;
            foreach (var item in _items[line])
            {
                if (item == null)
                    continue;

                prevLineLen += item.Length + 3;
            }

            prevLineLen -= 3;

            _items[line][column] = val.Replace(Environment.NewLine, string.Empty);

            var newLineLen = 0;
            foreach (var item in _items[line])
            {
                if (item == null)
                    continue;

                newLineLen += item.Length + 3;
            }

            newLineLen -= 3;

            if (prevLineLen > newLineLen)
            {
                var spaces = prevLineLen - newLineLen;
                TrimEndChars(line, prevLineLen - spaces, spaces);
            }

            var output = new StringBuilder();
            for (var i = 0; i < _items[line].Length; i++)
            {
                output.Append($"{_items[line][i]}");
                output.Append(" | ");
            }
            var trimmed = output.ToString().TrimEnd(' ', '|');
            output.Clear();
            output.Append(trimmed);

            Win32ConsoleNativeMethods.WriteConsoleOutputCharacter(StdOutPtr, output, (uint)output.Length,
                new Coord(0, (short)line), out _);
            output.Clear();
        }

        private static void TrimEndChars(int line, int col, int spaces)
        {
            var tmp = new StringBuilder();
            for (var i = 0; i < spaces; i++)
                tmp.Append('\0');

            Win32ConsoleNativeMethods.WriteConsoleOutputCharacter(StdOutPtr, tmp, (uint)tmp.Length,
                new Coord((short)col, (short)line), out _);
            tmp.Clear();
        }

        public void WriteFooter(string val)
        {
            if (Console.BufferHeight == _items.Count)
                Console.BufferHeight += val.Split('\n').Length;

            var split = val.Split(Environment.NewLine.ToCharArray());
            var y = (_items.Count + 1);
            foreach (var v in split)
            {
                if (string.IsNullOrWhiteSpace(v))
                    continue;

                var trimmed = v.Trim();
                //TODOClearLine(y,);
                TrimEndChars(y, 0, Console.BufferWidth);
                Win32ConsoleNativeMethods.WriteConsoleOutputCharacter(StdOutPtr, new StringBuilder(v.Trim()),
                    (uint)trimmed.Length,
                    new Coord(0, (short)y++), out _);
            }
        }
    }
}

#endif