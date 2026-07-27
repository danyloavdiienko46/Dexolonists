using Godot;
using System;

public partial class MultiplayerGameUi : Control
{
	[Export] public ColorRect TurnColourRect;
	[Export] public Label DiceRollLabel;

	public void ChangeTurnRectColour(Color colour)
	{
		TurnColourRect.Color = colour;
	}

	public void UpdateDiceRollLabel(int number)
	{
		DiceRollLabel.Text = number.ToString();
	}
}
