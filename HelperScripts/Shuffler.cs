using System;
using System.Collections.Generic;
using Godot;

namespace HelperScripts
{
    public class Shuffler
    {
        public void ShuffleArray<T>(T[] array)
        {
            Random random = new Random();

            for (int i = 0; i < array.Length - 1; ++i) 
            {
                int r = random.Next(i, array.Length);
                (array[r], array[i]) = (array[i], array[r]);
            }
        }

        public void ShuffleList<T>(List<T> list)
        {
            Random random = new Random();

            for (int i = 0; i < list.Count - 1; ++i) 
            {
                int r = random.Next(i, list.Count);
                (list[r], list[i]) = (list[i], list[r]);
            }
        }
    }
}
