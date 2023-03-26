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
		Label record = GetNode<Label>("Record");
		// use method GetRecordScore to get the record score from the binary file
		record.Text = GetRecordScore().ToString();
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
		// get record from label and compare with current score
		Label record = GetNode<Label>("Record");
		int recordScore = GetRecordScore();
		if (this.score > recordScore)
		{
			record.Text = this.score.ToString();
			SaveRecordScore();
		}
	}

	// A function that saves the record score by the end of a game and saves it to a binary file
	public void SaveRecordScore()
	{
		// Get the record score from the binary file
		int recordScore = GetRecordScore();
		// If the current score is greater than the record score, then save the current score as the record score
		if (this.score > recordScore)
		{
			// Create a new file
			FileAccess file = FileAccess.Open("user://recordScore.dat", FileAccess.ModeFlags.Write);

			Json recordSave = new Json();
			recordSave.Data = new Godot.Collections.Dictionary<string, Variant>();
			((Godot.Collections.Dictionary)recordSave.Data).Add("recordScore", this.score);

			Godot.Collections.Dictionary<string,Variant> prevRecordSaved = GetRecordLine();
			if (prevRecordSaved != null)
			{
				((Godot.Collections.Dictionary)recordSave.Data).Merge((Godot.Collections.Dictionary)prevRecordSaved);
			}

			// Write the record score to the file
			file.StoreLine(Json.Stringify(recordSave.Data));

			// Close the file
			file.Close();
		}
	}

	private Godot.Collections.Dictionary<string, Variant> GetRecordLine()
	{
		var dir = DirAccess.Open("user://");
		// If the file exists, then open it in read mode
		if (dir.FileExists("recordScore.dat"))
		{
			FileAccess file = FileAccess.Open("user://recordScore.dat", FileAccess.ModeFlags.Read);
			// Read the record score from the file
			string savedLine = file.GetLine();

			Json recordSave = new Json();
			var error = recordSave.Parse(savedLine);
			if (error != Error.Ok) {
				return null;
			}
			var SavedData = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)recordSave.Data);
			// Close the file
			file.Close();
			return SavedData;
		}
		else
		{
			return null;
		}
	}

	// GetRecordScore() returns the record score from the binary file
	public int GetRecordScore()
	{
		var SavedData = GetRecordLine();
		if (SavedData != null)
		{
			int recordScore = 0;
			if (SavedData.ContainsKey("recordScore"))
			{
				recordScore = (int)SavedData["recordScore"];
			}
			// Return the record score
			return recordScore;
		}
		// If the file doesn't exist, then return 0
		else
		{
			return 0;
		}
	}

}
