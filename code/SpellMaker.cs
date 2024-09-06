using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

public sealed class SpellMaker : Component
{
	[Property] public GameObject DraggedNode { get; set; }

	[Property] public NodeOutput CurrentOutput { get; set; }

	[Property] public NumberInput CurrentNumberInput { get; set; }
	[Property] public float CostCapacity { get; set; } = 100f;

	[Property] public InteractionState CurrentInteractionState { get; set; }
	[Property] public GameObject SpawnRef { get; set; }
	

	List<Node> _nodes;
	public List<Node> Nodes
	{
		get
		{
			if(_nodes != null && !nodesUpdated)
				return _nodes;
			
			_nodes = new List<Node>();
			foreach(GameObject c in GameObject.Children)
			{
				Node node = c.Components.Get<Node>(true);
				if(node == null) continue;
				_nodes.Add(node);
			}
			return _nodes;
		}
	}

	private Vector3 _mouseDragOrigin;
	private Vector3 _nodeDragOrigin;
	private float _dragDistance;

	private SceneTraceResult _mouseRay;
	private InteractionState _lastInteractionState;

	private List<Vector3> _temporaryDetours = new();

	bool nodesUpdated;

	public string GetSaveData()
	{
		JsonObject jsonObject = GameObject.Serialize();
		jsonObject.Remove("Component");
		SceneUtility.MakeIdGuidsUnique(jsonObject);
		return jsonObject.ToJsonString();
	}

	public bool DistributeMana(float totalMana)
	{
		if(Mana()+totalMana > MaxMana() || Mana()+totalMana < 0)
			return false;
		
		var validNodes = Nodes.Where(node => node.Mana > 0).ToList();
		if (!validNodes.Any()) return false;

		float manaPerNode = totalMana / validNodes.Count;

		float overflow = 0;
		foreach (var node in validNodes)
		{
			float newMana = node.Mana + manaPerNode + overflow;
			node.Mana = MathX.Clamp(newMana,0,node.MaxMana);
			if(node.Mana != newMana)
				overflow = 0;
			else
				overflow = node.Mana - newMana;
		}

		return true;
	}

	public float MaxMana()
	{
		float max = 0;
		foreach(var node in Nodes)
		{
			max += node.MaxMana;
		}
		return max;
	}

	public float Mana()
	{
		float mana = 0;
		foreach(var node in Nodes)
		{
			mana += node.Mana;
		}
		return mana;
	}

	public const string PrefabDir = "prefabs/nodes";
	public static (List<string> paths, List<string> names, List<float> prices) GetPrefabs()
	{

		List<string> paths = FileSystem.Mounted.FindFile(PrefabDir, "*.prefab").ToList(); 
		List<string> names = new List<string>();
		List<float> prices = new List<float>();

		foreach(string p in paths)
		{
			string name = p.Replace(".prefab","");
			names.Add(ToFriendlyCase(name));
			PrefabFile prefabFile = ResourceLibrary.Get<PrefabFile>($"{PrefabDir}/{p}");
			JsonDocument doc = JsonDocument.Parse(prefabFile.RootObject.ToJsonString());
			JsonElement root = doc.RootElement;
			foreach (JsonElement component in root.GetProperty("Components").EnumerateArray())
			{
				if (component.TryGetProperty("__type", out JsonElement typeElement) && typeElement.GetString().Contains("Node"))
				{
					if (component.TryGetProperty("Cost", out JsonElement priceElement))
					{
						prices.Add((float)priceElement.GetDecimal());
					}
					break;
				}
			}
		}

		return(paths,names,prices);
	}

	public static string ToFriendlyCase(string PascalString)
    {
        return Regex.Replace(PascalString, "(?!^)([A-Z])", " $1");
    }
	public float price {get; set;}
	public float GetPrice()
	{
		float sum = 0;
		foreach(GameObject c in GameObject.Children)
		{
			Node node = c.Components.Get<Node>();
			if(node == null) continue;
			sum += node.Cost;
		}
		price = sum;
		return sum;
	}
	public void CreateSpell(string file)
	{
		
		if ( !ResourceLibrary.TryGet($"{PrefabDir}/{file}", out PrefabFile prefab)) return;
		var spawned = SceneUtility.GetPrefabScene( prefab ).Clone();
		Node node = spawned.Components.Get<Node>();
		if(GetPrice()+node.Cost > CostCapacity)
		{
			Sound.Play("sounds/player_use_fail.sound");
			spawned.DestroyImmediate();
			return;
		}
		spawned.SetParent(GameObject);
		nodesUpdated = true;
		spawned.Transform.LocalPosition = SpawnRef.Transform.LocalPosition;
		spawned.Transform.LocalRotation = SpawnRef.Transform.LocalRotation;
	}

	protected override void OnPreRender()
	{
		if(Input.UsingController)
		{
			Mouse.Position -= new Vector2(Input.AnalogMove.y, Input.AnalogMove.x) * (Input.Down("View") ? 1f : 2.5f) * (Screen.Size.Length/1500f);

		}

		var mousePosition = Mouse.Position;

		var camera = Scene.Camera;

		_mouseRay = Scene.Trace.Ray( camera.ScreenPixelToRay( mousePosition ), 50 )
							.HitTriggersOnly()
							.Run();

		switch ( CurrentInteractionState )
		{
			case InteractionState.Finding:
				HandleFindingState();
				break;

			case InteractionState.Dragging:
				HandleDraggingState();
				break;

			case InteractionState.Connecting:
				HandleConnectingState();
				break;

			case InteractionState.TypingNumber:
				HandleTypingNumberState();
				break;
		}

		_lastInteractionState = CurrentInteractionState;
	}
	
	private void HandleFindingState()
	{
		if ( !_mouseRay.Hit ) return;

		if ( Input.Pressed( "attack2" ) )
		{
			if ( _mouseRay.GameObject.Tags.Contains( "output" ) )
			{
				var nodeOutput = _mouseRay.GameObject.Components.Get<NodeOutput>();
				if ( nodeOutput.Connections.Count > 0 )
				{
					nodeOutput.Connections.RemoveAt( nodeOutput.Connections.Count - 1 );
				}
			}
			else if (_mouseRay.GameObject.Tags.Contains( "node" ))
			{
				_mouseRay.GameObject.DestroyImmediate();
				nodesUpdated = true;
				GetPrice();
				return;
			}
		}

		if ( !Input.Pressed( "attack1" ) ) return;

		if(_mouseRay.GameObject.Tags.Contains("numberinput"))
		{
			CurrentNumberInput = _mouseRay.GameObject.Components.Get<NumberInput>();
			if(!CurrentNumberInput.IsValid()) return;
			CurrentInteractionState = InteractionState.TypingNumber;
			return;
		}

		if ( _mouseRay.GameObject.Tags.Contains( "input" ) ) return;

		if ( _mouseRay.GameObject.Tags.Contains( "output" ) )
		{
			CurrentOutput = _mouseRay.GameObject.Components.Get<NodeOutput>();
			if(!CurrentOutput.IsValid()) return;
			CurrentInteractionState = InteractionState.Connecting;

			Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
			Vector3 cameraPosition = Scene.Camera.Transform.Position;

			_dragDistance = Vector3.Dot( _mouseRay.HitPosition - cameraPosition, cameraForward );
			return;
		}

		if ( _mouseRay.GameObject.Tags.Contains( "node" ) )
		{
			DraggedNode = _mouseRay.GameObject;

			Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
			Vector3 cameraPosition = Scene.Camera.Transform.Position;

			_dragDistance = Vector3.Dot( _mouseRay.HitPosition - cameraPosition, cameraForward );

			_mouseDragOrigin = _mouseRay.HitPosition;
			_nodeDragOrigin = DraggedNode.Transform.Position;

			CurrentInteractionState = InteractionState.Dragging;
		}
	}

	private void HandleDraggingState()
	{
		if ( Input.Released( "attack1" ) )
		{
			CurrentInteractionState = InteractionState.Finding;
			return;
		}

		var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );

		Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
		Vector3 projectedPosition = ray.Position + ray.Forward * (_dragDistance / Vector3.Dot( ray.Forward, cameraForward ));

		DraggedNode.Transform.Position = _nodeDragOrigin + (projectedPosition - _mouseDragOrigin);

		float RollAmount;

		if(Input.Pressed("use"))
			RollAmount = -90;
		else if (Input.Pressed("reload"))
			RollAmount = 90;
		else
			return;

		DraggedNode.Transform.LocalRotation = DraggedNode.Transform.LocalRotation.Angles().WithRoll(DraggedNode.Transform.LocalRotation.Angles().roll + RollAmount);
	}

	private void HandleConnectingState()
	{
		if ( Input.Down( "attack2" ) )
		{
			CurrentInteractionState = InteractionState.Finding;
			return;
		}

		var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );

		Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
		Vector3 projectedPosition = ray.Position + ray.Forward * (_dragDistance / Vector3.Dot( ray.Forward, cameraForward ));

		CurrentOutput.DrawLineTo( projectedPosition);

		if ( Input.Pressed( "attack1" ) )
		{
			if ( _mouseRay.Hit && _mouseRay.GameObject.Tags.Contains( "input" ) )
			{
				var nodeInput = _mouseRay.GameObject.Components.Get<NodeInput>();
				if(CurrentOutput.outputType == nodeInput.AcceptedType)
				{
					CurrentOutput.Connections.Add( 
						new NodeOutput.Connection{ ConnectedObject = nodeInput.node.GameObject, Index = nodeInput.index }
						);
					Log.Info(CurrentOutput.Connections.Count);
					CurrentInteractionState = InteractionState.Finding;
					return;
				}
			}
		}
	}

	private void HandleTypingNumberState()
	{
		
		if(Input.Pressed("Chat"))
		{
			float number;
			if(float.TryParse(CurrentNumberInput.TextRenderer.Text, out number))
			{
				CurrentNumberInput.Value = number;
				CurrentNumberInput.TextRenderer.Text = number.ToString();
				CurrentNumberInput.TextRenderer.Color = CurrentNumberInput.TextRenderer.Color.WithAlpha(1);
				CurrentInteractionState = InteractionState.Finding;
				return;
			}
			else
			{
				Sound.Play("sounds/player_use_fail.sound");
			}
		}

		float a = (MathF.Sin(Time.Now*10)+1)/2;
		CurrentNumberInput.TextRenderer.Color =CurrentNumberInput.TextRenderer.Color.WithAlpha(a);

		void IncreaseNumber(float amount)
		{
			float number;
			if(float.TryParse(CurrentNumberInput.TextRenderer.Text, out number))
			{
				CurrentNumberInput.Value = MathF.Round((number+amount)*10) / 10;
				CurrentNumberInput.TextRenderer.Text = CurrentNumberInput.Value.ToString();
			}
			else
			{
				Sound.Play("sounds/player_use_fail.sound");
			}
		}

		if(Input.Pressed("SlotPrev"))
		{
			IncreaseNumber(-0.1f);
			return;
		}
		if(Input.Pressed("SlotNext"))
		{
			IncreaseNumber(0.1f);
			return;
		}

		if(Input.Pressed("Back"))
		{
			if(CurrentNumberInput.TextRenderer.Text.Length > 0)
				CurrentNumberInput.TextRenderer.Text = CurrentNumberInput.TextRenderer.Text.Remove(CurrentNumberInput.TextRenderer.Text.Length-1);
			return;
		}

		char added = ' ';
		bool input = false;
		for(int i = 0; i < 10; i++)
		{
			if(Input.Pressed($"Slot{i}"))
			{
				added = i.ToString()[0];
				input = true;
				break;
			}
		}
		if(Input.Pressed("Point"))
		{
			added = '.';
			input = true;
		}
		if(Input.Pressed("Minus"))
		{
			added = '-';
			input = true;
		}

		if(!input) return;
		CurrentNumberInput.TextRenderer.Text += added;
	}

	public enum InteractionState
	{
		Finding,
		Dragging,
		Connecting,
		TypingNumber
	}
}