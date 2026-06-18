using System.Collections.Generic;
using Godot;

namespace HelperScripts
{
    public class Dictionaries
    {
        public Dictionary<int, Vector3> TPP_ind_to_pos = new Dictionary<int, Vector3>();
        public Dictionaries()
        {
            TPP_ind_to_pos.Add(0, new Vector3(-1.58f, 0, -2.735f));
            TPP_ind_to_pos.Add(1, new Vector3(1.58f, 0, -2.735f));
            TPP_ind_to_pos.Add(2, new Vector3(3.16f, 0, 0));
            TPP_ind_to_pos.Add(3, new Vector3(1.58f, 0, 2.735f));
            TPP_ind_to_pos.Add(4, new Vector3(-1.58f, 0, 2.735f));
            TPP_ind_to_pos.Add(5, new Vector3(-3.16f, 0, 0));
        }
    }
}
