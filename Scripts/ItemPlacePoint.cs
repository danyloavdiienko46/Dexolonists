using dexolonists.Scripts.Enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class ItemPlacePoint : Area3D
{
	[Export] public bool IsActive {get; set;} = true;

	[Export] public bool IsHousePlacementPermitted {get; set;} = true;

	public List<ResourceCardType> resource_gain_list = new List<ResourceCardType>();
	private bool _is_timer_created = false;
	public bool gives_gold = false;

    public override void _PhysicsProcess(double delta)
    {
		if (!_is_timer_created)
		{
			_is_timer_created = true;
			var timer = GetTree().CreateTimer(0.5f);
			timer.Timeout += () => 
			{
				var bodies = GetOverlappingBodies();
				foreach(var body in bodies)
				{
					if(body is Tile tile)
					{
						if(tile.type == TileType.Gold)
							gives_gold = true;
						else if(tile.type != TileType.Desert &&
							tile.type != TileType.Empty)
							{
								resource_gain_list.Add((ResourceCardType)(tile.type-1));
							}
					}
				}
			};
			
		}
			
    }

}
