using Godot;
using System;

public partial class PlayerLobbyIcon : Panel
{
	private RichTextLabel _label;

    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("RichTextLabel");
		_label.Text = $"Player#{Name}";
    }

}
