using Sandbox;

public sealed class SpellMaker : Component
{
	[Property] GameObject DraggedNode;
	Vector3 MouseDragOrigin;
	Vector3 NodeDragOrigin;
	float DragDis;

	[Property]NodeOutput currentOutput;

	[Property] InteractState interactState;
	
	enum InteractState
	{
		Finding,
		Dragging,
		ConnectingB
	}
	SceneTraceResult mouseRay;

	InteractState lastInteractState;
	protected override void OnUpdate()
	{
	
		Mouse.Visible = true;	
		var mouse = Mouse.Position;
		var camera = Scene.Camera;
		mouseRay = Scene.Trace.Ray(camera.ScreenPixelToRay(mouse),50).HitTriggersOnly().Run();

		switch (interactState)
		{
			case InteractState.Finding:
			{Finding();break;}
			case InteractState.Dragging:
			{Dragging();break;}
			case InteractState.ConnectingB:
			{ConnectingB();break;}
		}

		lastInteractState = interactState;
	}

	void Finding()
	{
		if(!mouseRay.Hit) return;

		if(Input.Pressed("attack2"))
		{
			if(mouseRay.GameObject.Tags.Contains("output"))
			{
				NodeOutput nodeOutput = mouseRay.GameObject.Components.Get<NodeOutput>();
				if(nodeOutput.Connected.Count > 0)
				{
					nodeOutput.Connected.RemoveAt(nodeOutput.Connected.Count-1);
					nodeOutput.Detours.RemoveAt(nodeOutput.Detours.Count-1);
				}
			}
		}

		if(!Input.Pressed("attack1")) return;
		if(mouseRay.GameObject.Tags.Contains("input")) return;

		if(mouseRay.GameObject.Tags.Contains("output"))
		{
			currentOutput = mouseRay.GameObject.Components.Get<NodeOutput>();
			interactState = InteractState.ConnectingB;
			Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
			Vector3 cameraPosition = Scene.Camera.Transform.Position;

			DragDis = Vector3.Dot(mouseRay.HitPosition - cameraPosition, cameraForward);
			
			return;
		}

		if(mouseRay.GameObject.Tags.Contains("node"))
		{
			DraggedNode = mouseRay.GameObject;
			
			Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
			Vector3 cameraPosition = Scene.Camera.Transform.Position;

			DragDis = Vector3.Dot(mouseRay.HitPosition - cameraPosition, cameraForward);

			MouseDragOrigin = mouseRay.HitPosition;

			NodeDragOrigin = DraggedNode.Transform.Position;

			interactState = InteractState.Dragging;
			return;
		}
	}
	void Dragging()
	{
		if(Input.Released("attack1"))
		{
			interactState = InteractState.Finding;
			return;
		}
		if(Input.Pressed("use"))
		{
			DraggedNode.Transform.LocalRotation = DraggedNode.Transform.LocalRotation.Angles().WithRoll(DraggedNode.Transform.LocalRotation.Angles().roll + 90);
		}

		var mouseRay = Scene.Camera.ScreenPixelToRay(Mouse.Position);

		Vector3 cameraForward = Scene.Camera.Transform.World.Forward;

		Vector3 projectedPosition = mouseRay.Position + mouseRay.Forward * (DragDis / Vector3.Dot(mouseRay.Forward, cameraForward));

		DraggedNode.Transform.Position = NodeDragOrigin + (projectedPosition - MouseDragOrigin);

	}
	List<Vector3> TempDetours = new List<Vector3>();
	void ConnectingB()
	{
		if(Input.Down("attack2"))
		{
			interactState = InteractState.Finding;
			return;
		}

		var ray = Scene.Camera.ScreenPixelToRay(Mouse.Position);

		Vector3 cameraForward = Scene.Camera.Transform.World.Forward;

		Vector3 projectedPosition = ray.Position + ray.Forward * (DragDis / Vector3.Dot(ray.Forward, cameraForward));

		currentOutput.DrawLineTo(projectedPosition,TempDetours);

		if(Input.Pressed("attack1"))
		{
			if(mouseRay.Hit)
			{
				if(mouseRay.GameObject.Tags.Contains("input"))
				{
					NodeInput nodeInput = mouseRay.GameObject.Components.Get<NodeInput>();
					currentOutput.Connected.Add(nodeInput);
					currentOutput.Detours.Add(TempDetours);
					TempDetours = new List<Vector3>();
					interactState = InteractState.Finding;
					return;
				}
			}

			TempDetours.Add(projectedPosition);
		}

		
	}
}
