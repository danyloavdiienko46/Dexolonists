using Godot;
using System;

public partial class MultiplayerGameUi : Control
{
	[Export] public ColorRect TurnColourRect;

	public void ChangeTurnRectColour(Color colour)
	{
		TurnColourRect.Color = colour;
	}
}
