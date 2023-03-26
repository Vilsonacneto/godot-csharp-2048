using Godot;
using System;

public partial class Element : Control
{
	public int value;
	public Vector2 boardPosition;
	private int prevValue;
	private Label valueLabel;
	private Panel backGround;

	public bool HasChanged;

	private Godot.Color DefaultColor = Godot.Colors.DarkGray;
	private Godot.Color OutOfLimitColor = Godot.Colors.Purple;

	[Signal]
	public delegate void AddScoreEventHandler(int value);

	private Godot.Collections.Dictionary<int,Godot.Color> ColorSet = new Godot.Collections.Dictionary<int, Godot.Color>{
		{0, Godot.Colors.LightSlateGray},
		{2, Godot.Colors.Aqua},
		{4, Godot.Colors.Violet},
		{8, Godot.Colors.Magenta},
		{16, Godot.Colors.Crimson},
		{32, Godot.Colors.Orange},
		{64, Godot.Colors.Gold},
		{128, Godot.Colors.GreenYellow}
	};

	public void SetValue(int value)
	{
		this.value = value;
		this.valueLabel.Text = this.value.ToString();
		this.HasChanged = true;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.prevValue = 0;
		this.valueLabel = GetNode<Label>("BackGround/Value");
		this.backGround = GetNode<Panel>("BackGround");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (prevValue != value)
		{
			Color color;
			if (this.valueLabel.Text.Length > 0) {
				color = ColorSet.ContainsKey(this.value) ? ColorSet[this.value] : OutOfLimitColor;
			}
			else
			{
				color = DefaultColor;
			}
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(this.backGround,"modulate", color, 0.1f);
			this.prevValue = value;
		}
	}

	public void Reset()
	{
		this.value = 0;
		this.valueLabel.Text = "";
	}

	public void Add(Element e)
	{
		if (this.value != e.value && this.value != 0) {
			this.HasChanged = false;
			return;
		} 
		var newValue = e.value + this.value;
		if (merged(this,e)) EmitSignal(SignalName.AddScore, newValue);
		this.SetValue(newValue);
		
		if (this.value == 0) this.Reset();
		
	}

	private bool merged(Element a, Element b)
	{
		return a.value == b.value && a.value != 0 && b.value != 0;
	}

	public void MoveTo(Element e)
	{
		e.Add(this);
		if (!e.HasChanged) return;
		this.Reset();
		this.HasChanged = true;
	}

	public bool Empty()
	{
		return this.valueLabel.Text.Length == 0;
	}
}
