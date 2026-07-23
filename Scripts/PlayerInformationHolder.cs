using dexolonists.Scripts.Enums;
using Godot;
using System;

using System.Collections.Generic;

public partial class PlayerInformationHolder : Node
{
	private int _points = 0;
	private int[] _resource_card_array = [0, 0, 0, 0, 0];

	public void AddOrSubtractResource(ResourceCardType type, int number, bool subtracting = false)
	{
		int num = number;
		if(subtracting) num*=-1;

		_resource_card_array[(int)type] += num;
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
		return _points;
	}

	public void AddOrSubtractPoints(int number, bool subtracting = false)
	{
		int num = number;
		if(subtracting) num *= -1;

		_points += num;
	}
}
