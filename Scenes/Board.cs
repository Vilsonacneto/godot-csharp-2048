using Godot;
using System;

// TODO: (bug) o número precisa ir até o fim da linha ou coluna
public partial class Board : PanelContainer
{
	private bool started = false;
	private bool finished = false;
	private bool moving = false;
	PackedScene element;
	Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int,Element>> elements = new Godot.Collections.Dictionary<int,Godot.Collections.Dictionary<int,Element>>{};
	ElementIterator iterator;
	Vector2 MinElementSize  = new Vector2(64,64);

	Panel panel;

	int DefaultGap = 10;

	public static Vector2 LEFT = new Vector2(-1,0);
	public static Vector2 RIGHT = new Vector2(1,0);
	public static Vector2 UP = new Vector2(0,-1);
	public static Vector2 DOWN = new Vector2(0,1);
	
	public const int MAX_INDEX = 3;
	public const int MIN_INDEX = 0;

	private const double MOVING_DELAY = 0.05f;

	[Signal]
	public delegate void StatusChangedEventHandler(bool finished);

	[Signal]
	public delegate void AddScoreProxyEventHandler(int value);

	public void OnProxyAddScore(int value)
	{
		EmitSignal(SignalName.AddScoreProxy, value);
	}

	private void organize()
	{
		var maxSize = new Vector2(0,0); 
		foreach (Element e in iterator)
		{
			var xpos = (e.boardPosition.X * MinElementSize.X) + (e.boardPosition.X + DefaultGap);
			var ypos = (e.boardPosition.Y * MinElementSize.Y) + (e.boardPosition.Y + DefaultGap);
			var pos = new Vector2(xpos, ypos);
			e.Position = pos;
			maxSize = updateMaxSize(maxSize, pos);
		}
		maxSize = finalSize(maxSize);
		panel.Size = maxSize;
	}

	private Vector2 updateMaxSize(Vector2 v, Vector2 update)
	{
		if (update.X > v.X)
		{
			v.X = update.X;
		}
		if (update.Y > v.Y)
		{
			v.Y = update.Y;
		}
		return v;
	}

	private Vector2 finalSize(Vector2 v)
	{
		v.X = v.X + MinElementSize.X;
		v.Y = v.Y + MinElementSize.Y;
		return v;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		panel = GetNode<Panel>("/root/Main/Panel");

		element = (PackedScene)GD.Load("res://Scenes/Element/Element.tscn");
		for (int x = MIN_INDEX; x <= MAX_INDEX; x++)
		{
			for (int y = MIN_INDEX; y <= MAX_INDEX; y++)
			{
				Element newEl = (Element)element.Instantiate();
				newEl.Size = this.MinElementSize;
				newEl.boardPosition = new Vector2(x,y);
				panel.AddChild(newEl);
				if (y == 0) {
					this.elements.Add(x, new Godot.Collections.Dictionary<int, Element>{});
				}
				this.elements[x].Add(y, newEl);
				newEl.AddScore += value => this.OnProxyAddScore(value);
			}
		}
		this.iterator = new ElementIterator(this.elements);
		this.organize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!started || finished || moving) return;
		if (Input.IsActionJustReleased("left"))
		{
			Move(LEFT);
		}
		if (Input.IsActionJustReleased("right"))
		{
			Move(RIGHT);
		}
		if (Input.IsActionJustReleased("up"))
		{
			Move(UP);
		}
		if (Input.IsActionJustReleased("down"))
		{
			Move(DOWN);
		}
		if (Input.IsActionJustReleased("finish"))
		{
			changeStatus(true);
		}
	}
	
	private void changeStatus(bool finish)
	{
		this.finished = finish;
		EmitSignal(SignalName.StatusChanged, finish);
	}

	public async void OnButtonPressed()
	{
		if (started)
		{
			changeStatus(true);
			Popup popup = GetNode<Popup>("/root/Main/Popup");
			await ToSignal(popup, "popup_hide");
		}
		resetAllElements();

		uint rand = GD.Randi() % 4;
		int initialElements = 1;
		if (rand == 0) initialElements = 2;

		var initialScore = 0;

		for (int i = 0; i < initialElements; i++)
		{
			uint pickx = GD.Randi() % 4;
			uint picky = GD.Randi() % 4;
			var element = elements[((int)pickx)][(int)picky];
			var value = randomValue();
			initialScore += value;
			element.SetValue(value);
		}
		this.started = true;
		changeStatus(false);

		iterator.SetMode((iterator.GetMode() + 1) % 4);
	}
	private int randomValue()
	{
		var r = GD.Randi() % 1000;
		return r >= 750 && r <= 980 ? 4 : (r > 980 ? 8 : 2);
	}

	private void resetAllElements()
	{
		foreach (Element e in iterator)
		{
			e.Reset();
		}
	}

	private async void Move(Vector2 direction)
	{
		this.moving = true;
		var coordinatesToIgnore = ignore(direction);
		// Move all pieces to the limit, means moving it 3 times to the same direction
		for (int i = 0; i < 3; i++)
		{
			foreach (Element e in iterator)
			{
				if (e.boardPosition.X == coordinatesToIgnore.X || e.boardPosition.Y == coordinatesToIgnore.Y) continue;
				var moveTo = e.boardPosition + direction;
				var elementMove = elements[(int)moveTo.X][(int)moveTo.Y];
				e.MoveTo(elementMove);
			}
			await ToSignal(GetTree().CreateTimer(MOVING_DELAY), "timeout"); 
		}
		createNewElements();
		this.moving = false;
	}

	private Vector2 ignore(Vector2 direction)
	{
		var outOfIndex = MAX_INDEX+1;
		iterator.SetMode(mode(direction));
		if (direction.Equals(LEFT))
		{
			return new Vector2(0, outOfIndex);
		}
		else if (direction.Equals(RIGHT))
		{
			return new Vector2(3, outOfIndex);
		}
		else if (direction.Equals(UP))
		{
			return new Vector2(outOfIndex, 0);
		}
		else if (direction.Equals(DOWN))
		{
			return new Vector2(outOfIndex, 3);
		}
		return new Vector2(outOfIndex, outOfIndex);
	}

	private int mode(Vector2 direction)
	{
		if (direction.Equals(RIGHT))
		{
			return ElementIterator.REVERSE_X;
		}
		else if (direction.Equals(DOWN))
		{
			return ElementIterator.REVERSE_Y;
		}
		return ElementIterator.NORMAL;
	}

	private void createNewElements()
	{
		var empties = iterator.CountEmpties();
		if (empties == 0)
		{
			changeStatus(true);
			return;
		}
		var maxRandoms = Math.Min(2, empties);
		var newElements = (GD.Randi() % 1000) > 205 ? 1 : maxRandoms;
		for (int i = 0; i < newElements;)
		{
			var pickX = GD.Randi() % 4;
			var pickY = GD.Randi() % 4;
			var el = elements[(int)pickX][(int)pickY];
			if (el.Empty()) {
				var value = randomValue();
				el.SetValue(value);
				i++;
			}
		}
	}
}
