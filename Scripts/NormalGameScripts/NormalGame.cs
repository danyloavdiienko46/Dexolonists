using dexolonists.Scripts.Enums;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class NormalGame : Node3D
{
	[Export] public NormalGameBase NormalGameBaseNode;
	[Export] public PackedScene MainMenuScene;

	private MultiplayerWorld _mp_world;
	private bool _has_game_started = false;

	public bool has_map_just_loaded = false;

    public override void _Ready()
    {
		if(Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer) {
			_mp_world = GetParent<MultiplayerWorld>();
			
			LoadMapBase("res://Dexolonists_map.bin");
		}
    }

    public override void _PhysicsProcess(double delta)
    {
		if (!_has_game_started)
		{
			if (_mp_world.has_normal_game_started)
			{
				_has_game_started = true;
				NormalGameBaseNode.StartNormalGame();
			}
		}

		if (has_map_just_loaded)
		{
			_mp_world.has_map_just_loaded = true;

			has_map_just_loaded = false;
		}
    }

	public void LoadMapBase(string file_path)
	{
		NormalGameBaseNode.LoadMap(file_path);
	}

	public void ExitToMainMenu()
	{
		GetTree().ChangeSceneToPacked(MainMenuScene);
	}

	public void ItemChosen(int index, long player_ID = 0)
	{
		NormalGameBaseNode.ItemChosenHandler(index, player_ID);
	}

	public void NextTurn()
	{
		_mp_world.NextTurn();
	}

	public long GetCurrentTurnID()
	{
		return _mp_world.CurrentTurnID();
	}

	public int GetColourID(long id)
	{
		if(Multiplayer.MultiplayerPeer is not OfflineMultiplayerPeer)
			return _mp_world.GetColourID(id);
		else
			return 0;
	}

	//GameMechanics

	public int GetPlayerCardCount(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		return player_info_holder.GetAllResourcesNumber();
	}

	public int GetPlayerPoints(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		return player_info_holder.points;
	}

	public void ChangePlayerCardNumber(long player_ID, ResourceCardType resource_type, int number, bool subtracting = false)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		player_info_holder.AddOrSubtractResource(resource_type, number, subtracting);
	}

	private void ChangePlayerCardNumber(PlayerInformationHolder player_info_holder, ResourceCardType resource_type, int number, bool subtracting = false)
	{
		player_info_holder.AddOrSubtractResource(resource_type, number, subtracting);
	}

	public void ChangePlayerPoints(long player_ID, int number, bool subtracting = false)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		player_info_holder.AddOrSubtractPoints(number, subtracting);
	}

	private void ChangePlayerPoints(PlayerInformationHolder player_info_holder, int number, bool subtracting = false)
	{
		player_info_holder.AddOrSubtractPoints(number, subtracting);
	}

	public void PlayerBuildItem(long player_ID, Item item, ItemType item_type)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		if (player_info_holder.free_items_to_build.Contains(item_type))
		{
			Rpc(nameof(RpcSyncFreeItemsList), player_info_holder.GetPath(), (int)item_type, item.point_addition);
			return;
		}

		int[] building_cost = item.GetBuildingCost();

		for(int i = 0; i < building_cost.Length; i++)
		{
			if(building_cost[i] == 0) 
			{
				continue;
			}
			player_info_holder.AddOrSubtractResource((ResourceCardType)i, building_cost[i], subtracting: true);
			
		}

		ChangePlayerPoints(player_info_holder, item.point_addition);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void RpcSyncFreeItemsList(NodePath player_info_path, int int_item_type, int point_addition)
	{
		PlayerInformationHolder player_info_holder = GetNode<PlayerInformationHolder>(player_info_path);
		ItemType item_type = (ItemType)int_item_type;

		player_info_holder.free_items_to_build.Remove(item_type);
		ChangePlayerPoints(player_info_holder, point_addition);
	}

	public int[] GetPlayerDeck(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		return player_info_holder.GetCardArray();
	}

	public void AddIPPToPlayer(long player_ID, ItemPlacePoint IPP)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;
		
		player_info_holder.IPPs_in_jurisdiction.Add(IPP);
		foreach(RoadPlacePoint RPP in IPP.connected_RPPs_list)
		{
			player_info_holder.RPPs_in_jurisdiction.Add(RPP);
		}

		Rpc(nameof(RpcSyncPlayerIPPs), player_info_holder.GetPath(), IPP.GetPath());
	}

	public void AddIPPFromRPPToPlayer(long player_ID, RoadPlacePoint RPP)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;
		
		Rpc(nameof(RpcSyncPlayerRPPs), player_info_holder.GetPath(), RPP.GetPath());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void RpcSyncPlayerRPPs(NodePath player_info_path, NodePath rpp_path)
	{
		PlayerInformationHolder player_info_holder = GetNode<PlayerInformationHolder>(player_info_path);
		RoadPlacePoint RPP = GetNode<RoadPlacePoint>(rpp_path);

		foreach(ItemPlacePoint IPP in RPP.connected_IPPs_list)
		{
			if (!player_info_holder.IPPs_in_jurisdiction.Contains(IPP))
			{
				if (!player_info_holder.IPPs_in_theoretical_jurisdiction.Contains(IPP))
				{
					player_info_holder.IPPs_in_theoretical_jurisdiction.Add(IPP);
				}

				foreach(RoadPlacePoint road_place_point in IPP.connected_RPPs_list)
				{
					if (!player_info_holder.RPPs_in_jurisdiction.Contains(road_place_point))
					{
						player_info_holder.RPPs_in_jurisdiction.Add(road_place_point);
					}
				}
				
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	private void RpcSyncPlayerIPPs(NodePath player_info_path, NodePath ipp_path)
	{
		PlayerInformationHolder player_info_holder = GetNode<PlayerInformationHolder>(player_info_path);
		ItemPlacePoint IPP = GetNode<ItemPlacePoint>(ipp_path);

		player_info_holder.IPPs_in_jurisdiction.Add(IPP);
		foreach(RoadPlacePoint RPP in IPP.connected_RPPs_list)
		{
			if(!player_info_holder.RPPs_in_jurisdiction.Contains(RPP))
				player_info_holder.RPPs_in_jurisdiction.Add(RPP);
		}
	}

	public int[] GetPlayerRPPsIDs(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);

		if(player_index == -1)
		{
			GD.Print("Couldn't find player with ID: " + player_ID);
			return [];
		}
		
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;
		
		List<int> RPPsIDs = [];
		foreach(RoadPlacePoint RPP in player_info_holder.RPPs_in_jurisdiction)
		{
			RPPsIDs.Add(RPP.ID_in_game_list);
		}

		return RPPsIDs.ToArray();
	}

	public int[] GetPlayerTheoreticalIPPsIDs(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;
		
		List<int> IPPsIDs = [];
		foreach(ItemPlacePoint IPP in player_info_holder.IPPs_in_theoretical_jurisdiction)
		{
			IPPsIDs.Add(IPP.ID_in_game_list);
		}

		return IPPsIDs.ToArray();
	}

	public bool DoesPlayerHasForcedBuildingEnabled(long player_ID)
	{
		int player_index = _mp_world.players_IDs.IndexOf(player_ID);
		PlayerInformationHolder player_info_holder = _mp_world.normal_game_players.ElementAt(player_index).player_info_holder;

		return player_info_holder.is_forced_building_enabled;
	}

	public void NextStep()
	{
		_mp_world.NextSetupStep();
	}
}
