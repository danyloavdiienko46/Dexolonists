using Godot;
using System;

using System.Collections.Generic;
using HelperScripts;

public partial class PlayerLobbyIcon : Panel
{
	private RichTextLabel _label;
    
    private ColourList _colour_list = new ColourList();

    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("RichTextLabel");
		_label.Text = $"Player#{Name}";

        int colour_index = GetIndex();
        _label.Modulate = _colour_list.player_colors[colour_index];
    }

}
