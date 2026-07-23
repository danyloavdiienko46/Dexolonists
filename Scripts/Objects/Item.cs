using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public abstract partial class Item : StaticBody3D
{
    public int point_addition = 0;
    public abstract int[] GetBuildingCost();
}