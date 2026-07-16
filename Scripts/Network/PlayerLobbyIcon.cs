using Godot;
using System;

using System.Collections.Generic;

public partial class PlayerLobbyIcon : Panel
{
	private RichTextLabel _label;

    private readonly List<Color> _player_colors = new()
    {
        Colors.Red,
        Colors.Blue,
        Colors.Green,
        Colors.Yellow,
        Colors.Purple,
        Colors.Orange
    };

    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("RichTextLabel");
		_label.Text = $"Player#{Name}";

        int colour_index = GetIndex();
        _label.Modulate = _player_colors[colour_index];
    }

}
