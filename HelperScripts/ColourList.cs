using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace HelperScripts
{
    public class ColourList
    {
        public readonly List<Color> player_colors = new()
        {
            Colors.Red,
            Colors.Blue,
            Colors.Green,
            Colors.Yellow,
            Colors.Purple,
            Colors.Orange
        };
    }
}