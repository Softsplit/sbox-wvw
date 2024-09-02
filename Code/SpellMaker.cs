public sealed class SpellMaker : Component
{
	[Property] public GameObject DraggedNode { get; set; }

	[Property] public NodeOutput CurrentOutput { get; set; }

	[Property] public InteractionState CurrentInteractionState { get; set; }

	private Vector3 _mouseDragOrigin;
	private Vector3 _nodeDragOrigin;
	private float _dragDistance;

	private SceneTraceResult _mouseRay;
	private InteractionState _lastInteractionState;

	private List<Vector3> _temporaryDetours = new();

	protected override void OnUpdate()
	{
		Mouse.Visible = true;

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
				if ( nodeOutput.ConnectedInputs.Count > 0 )
				{
					nodeOutput.ConnectedInputs.RemoveAt( nodeOutput.ConnectedInputs.Count - 1 );
					nodeOutput.PathDetours.RemoveAt( nodeOutput.PathDetours.Count - 1 );
				}
			}
		}

		if ( !Input.Pressed( "attack1" ) ) return;
		if ( _mouseRay.GameObject.Tags.Contains( "input" ) ) return;

		if ( _mouseRay.GameObject.Tags.Contains( "output" ) )
		{
			CurrentOutput = _mouseRay.GameObject.Components.Get<NodeOutput>();
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

		CurrentOutput.DrawLineTo( projectedPosition, _temporaryDetours );

		if ( Input.Pressed( "attack1" ) )
		{
			if ( _mouseRay.Hit && _mouseRay.GameObject.Tags.Contains( "input" ) )
			{
				var nodeInput = _mouseRay.GameObject.Components.Get<NodeInput>();
				CurrentOutput.ConnectedInputs.Add( nodeInput );
				CurrentOutput.PathDetours.Add( _temporaryDetours );
				_temporaryDetours = new List<Vector3>();
				CurrentInteractionState = InteractionState.Finding;
				return;
			}

			_temporaryDetours.Add( projectedPosition );
		}
	}

	public enum InteractionState
	{
		Finding,
		Dragging,
		Connecting
	}
}