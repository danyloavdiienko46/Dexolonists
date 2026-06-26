using System.Collections.Generic;
using Godot;

namespace HelperScripts
{
    public class Dictionaries
    {
        public Dictionary<int, Vector3> TPP_ind_to_pos = new Dictionary<int, Vector3>();
        public Dictionary<int, float> RPP_ind_to_rot_degrees = new Dictionary<int, float>();
        public Dictionary<int, int> Tile_number_to_chances = new Dictionary<int, int>();
        public Dictionary<TileType, string> TileType_to_colour_code = new Dictionary<TileType, string>(); 
        public Dictionaries()
        {
            TPP_ind_to_pos.Add(0, new Vector3(-1.65f, 0, -2.74f));
            TPP_ind_to_pos.Add(1, new Vector3(1.65f, 0, -2.74f));
            TPP_ind_to_pos.Add(2, new Vector3(3.30f, 0, 0));
            TPP_ind_to_pos.Add(3, new Vector3(1.65f, 0, 2.74f));
            TPP_ind_to_pos.Add(4, new Vector3(-1.65f, 0, 2.74f));
            TPP_ind_to_pos.Add(5, new Vector3(-3.30f, 0, 0));

            RPP_ind_to_rot_degrees.Add(0, 120.0f);
            RPP_ind_to_rot_degrees.Add(1, 60.0f);
            RPP_ind_to_rot_degrees.Add(2, 0);
            RPP_ind_to_rot_degrees.Add(3, 120.0f);
            RPP_ind_to_rot_degrees.Add(4, 60.0f);
            RPP_ind_to_rot_degrees.Add(5, 0);

            Tile_number_to_chances.Add(-1, 1);
            Tile_number_to_chances.Add(2, 1);
            Tile_number_to_chances.Add(3, 2);
            Tile_number_to_chances.Add(4, 3);
            Tile_number_to_chances.Add(5, 4);
            Tile_number_to_chances.Add(6, 5);
            Tile_number_to_chances.Add(7, 6);
            Tile_number_to_chances.Add(8, 5);
            Tile_number_to_chances.Add(9, 4);
            Tile_number_to_chances.Add(10, 3);
            Tile_number_to_chances.Add(11, 2);
            Tile_number_to_chances.Add(12, 1);

            TileType_to_colour_code.Add(TileType.Empty, "#00FFFF");
            TileType_to_colour_code.Add(TileType.Tree, "#006400");
            TileType_to_colour_code.Add(TileType.Brick, "#DC143C");
            TileType_to_colour_code.Add(TileType.Wheat, "#FFD700");
            TileType_to_colour_code.Add(TileType.Sheep, "#7FFF00");
            TileType_to_colour_code.Add(TileType.Ore, "#A9A9A9");
            TileType_to_colour_code.Add(TileType.Desert, "#BDB76B");
            TileType_to_colour_code.Add(TileType.Gold, "#DAA520");
        }
    }
}
