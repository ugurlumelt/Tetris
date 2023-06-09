﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tetris
{
    public class GameGrid
    {

        private readonly int[,] grid;

        private MediaPlayer ClearBlockPlayer;
        public int Rows { get; }
        public int Columns { get; }

        public int this[int r, int c]
        {
            get => grid[r, c];
            set => grid[r, c] = value;

        }

        public GameGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];
            ClearBlockPlayer = new MediaPlayer();
            ClearBlockPlayer.Open(new Uri(Directory.GetCurrentDirectory() + "\\bob.wav"));
        }

        public bool IsInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Columns;
        }

        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }

        //which entire row is full
        public bool IsRowFull(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r,c]==0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsRowEmpty(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r, c] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        //clear full row
        private void ClearRow(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[r, c] = 0;
            }
        }

        private void MoveRowDown(int r, int numRows)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[r + numRows, c] = grid[r, c];
                grid[r, c] = 0;
            }
        }

        public int ClearFullRows()
        {
            int cleared = 0;

            for (int r = Rows-1;  r>= 0; r--)
            {
                if (IsRowFull(r))
                {
                    ClearRow(r);

                    ClearBlockPlayer.Position = TimeSpan.Zero;
                    ClearBlockPlayer.Play();

                    cleared++;
                }
                else if (cleared>0)
                {
                    MoveRowDown(r, cleared);
                }
            }
            return cleared;
        }

        public void StopSound()
        {
            ClearBlockPlayer.Volume = 0;
        }

        public void PlaySound()
        {
            ClearBlockPlayer.Volume = 1;
        }
    }
}
