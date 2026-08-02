using dexolonists.Scripts.Enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

public partial class ItemPlacePoint : Area3D
{
	[Export] public bool IsActive {get; set;} = true;

	[Export] public bool IsHousePlacementPermitted {get; set;} = true;

	[Export] public Area3D RoadObtainingArea;

	public List<ResourceCardType> resource_gain_list = new List<ResourceCardType>();
	public List<int> resource_roll_number_list = new List<int>();
	public List<RoadPlacePoint> connected_RPPs_list = new List<RoadPlacePoint>();
	private bool _is_timer_created = false;
	public bool gives_gold = false;

	public int ID_in_game_list = -1;

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
								resource_roll_number_list.Add(tile.number);
							}
					}
				}

				var areas = RoadObtainingArea.GetOverlappingAreas();
				foreach(var area in areas)
				{
					if(area is RoadPlacePoint RPP)
					{
						connected_RPPs_list.Add(RPP);
						RPP.connected_IPPs_list.Add(this);
					}
				}
			};
			
		}
			
    }

}
