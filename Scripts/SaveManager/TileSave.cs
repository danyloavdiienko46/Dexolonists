using Godot;
using System.Collections.Generic;

public partial class TileSave
{
	public Vector3 Position { get; set; }
	public int Type {get; set;}
    public string ScenePath { get; set; }
	public List<ItemPlacePointSave> item_place_point_saves {get; set;} = new List<ItemPlacePointSave>();

	public Godot.Collections.Dictionary ToDictionary()
	{
		var IPPs = new Godot.Collections.Array();
        foreach (var item in item_place_point_saves)
        {
            IPPs.Add(item.ToDictionary());
        }

        return new Godot.Collections.Dictionary
        {
            { "position", Position },
            { "scene_path", ScenePath },
            { "IPPs", IPPs },
			{"type", Type}
        };
	}

	public static TileSave FromDictionary(Godot.Collections.Dictionary dict)
    {
        var data = new TileSave
        {
            Position = (Vector3)dict["position"],
            ScenePath = (string)dict["scene_path"],
			Type = (int)dict["type"]
        };

        var IPPs = (Godot.Collections.Array)dict["IPPs"];
        foreach (Godot.Collections.Dictionary IPPdict in IPPs)
        {
            data.item_place_point_saves.Add(ItemPlacePointSave.FromDictionary(IPPdict));
        }

        return data;
    }

}
