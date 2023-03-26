using Godot;
using System;

public partial class Main : Node2D
{
	private bool finished = false;

	private int score;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("Quit").Pressed += () => GetTree().Quit();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Label score = GetNode<Label>("Score");
		score.Text = this.score.ToString();
		if (Input.IsActionJustReleased("start"))
		{
			GetNode<Board>("Board").OnButtonPressed();
		}
		if (Input.IsActionJustReleased("quit"))
		{
			GetTree().Quit();
		}
	}

	public void OnClosePressed()
	{
		Popup popup = GetNode<Popup>("Popup");
		popup.Visible = false;
	}

	public void OnStatusChanged(bool finished)
	{
		this.finished = finished;

		Popup popup = GetNode<Popup>("Popup");
		Label msg = GetNode<Label>("Popup/PopupMsg");
		if (finished)
		{
			msg.Text = $"Game over!\nYour score was: {this.score}";
			popup.Visible = true;
		} else
		{
			popup.Visible = false;
			this.score = 0;
		}
	}

	public void OnScoreChange(int newValue)
	{
		this.score += newValue;
	}

}
