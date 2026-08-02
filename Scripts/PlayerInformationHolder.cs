using dexolonists.Scripts.Enums;
using Godot;
using System;

using System.Collections.Generic;

public partial class PlayerInformationHolder : Node
{	
	private int _points_memory = 0;

	public List<ItemPlacePoint> IPPs_in_jurisdiction = new List<ItemPlacePoint>();
	public List<ItemPlacePoint> IPPs_in_theoretical_jurisdiction = new List<ItemPlacePoint>();
	public List<RoadPlacePoint> RPPs_in_jurisdiction = new List<RoadPlacePoint>();
	[Export] public int points
	{
		get => _points_memory;
		set
		{
			_points_memory = value;
			GetParent<NormalGamePlayer>().UpdatePointsPanelUI();
		}	
	}
	private int[] _resource_card_memory = [5, 5, 5, 5, 5];
	[Export] public int[] _resource_card_array
	{
		get => _resource_card_memory;
		set
		{
			_resource_card_memory = value;
			GetParent<NormalGamePlayer>().UpdateDeckPanelUI();
		}
	}
	private NormalGamePlayer _player; 

    public override void _Ready()
    {
        GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(1);
		_player = GetParent<NormalGamePlayer>();
    }


	public void AddOrSubtractResource(ResourceCardType type, int number, bool subtracting = false)
	{
		int num = number;
		if(subtracting) num*=-1;

		int[] updated_array = (int[])_resource_card_array.Clone();
        updated_array[(int)type] += num;
        _resource_card_array = updated_array;

		_player.UpdateDeckPanelUI();
	}

	public bool IsEnoughResource(ResourceCardType type, int number) //returns false if there is less then "number" of resource of type
	{
		return true ? (_resource_card_array[(int)type]-number) >= 0 : false;
	}

	public int GetResourceNumber(ResourceCardType type)
	{
		return _resource_card_array[(int)type];
	}

	public int GetAllResourcesNumber()
	{
		int sum = 0;
		foreach(int number in _resource_card_array)
		{
			sum += number;
		}

		return sum;
	}

	public int GetPoints()
	{
		return points;
	}

	public void AddOrSubtractPoints(int number, bool subtracting = false)
	{
		int num = number;
		if(subtracting) num *= -1;

		int new_points = points;
		new_points += num;
		points = new_points;
	}

	public int[] GetCardArray()
	{
		return _resource_card_array;
	}

	public bool CanPlayerBuildItem(Item item)
	{
		int[] building_cost = item.GetBuildingCost();

		for(int i = 0; i < building_cost.Length; i++)
		{
			if(building_cost[i] == 0) continue;

			if(!IsEnoughResource((ResourceCardType)i, building_cost[i]))
				return false;
		}
		
		return true;
	}

	public void GetTurnResources(int roll_number)
	{
		foreach(ItemPlacePoint IPP in IPPs_in_jurisdiction)
		{
			GD.Print("Entering foreach!");
			for(int i = 0; i < IPP.resource_gain_list.Count; i++)
			{
				GD.Print("Entering for!");
				if(IPP.resource_roll_number_list[i] == roll_number)
				{
					GD.Print("Adding resources!");
					AddOrSubtractResource(IPP.resource_gain_list[i], 1);
				}
			}
		}
	}
}
