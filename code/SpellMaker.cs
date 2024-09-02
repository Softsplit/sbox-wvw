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
		Connecting
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
			case InteractState.Connecting:
			{Connecting();break;}
		}

		lastInteractState = interactState;
	}

	void Finding()
	{
		if(!Input.Down("attack1")) return;

		if(!mouseRay.Hit) return;
		if(mouseRay.GameObject.Tags.Contains("input")) return;

		if(mouseRay.GameObject.Tags.Contains("output"))
		{
			currentOutput = mouseRay.GameObject.Components.Get<NodeOutput>();
			interactState = InteractState.Connecting;
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
		var mouseRay = Scene.Camera.ScreenPixelToRay(Mouse.Position);

		Vector3 cameraForward = Scene.Camera.Transform.World.Forward;

		Vector3 projectedPosition = mouseRay.Position + mouseRay.Forward * (DragDis / Vector3.Dot(mouseRay.Forward, cameraForward));

		DraggedNode.Transform.Position = NodeDragOrigin + (projectedPosition - MouseDragOrigin);

	}
	List<Vector3> TempDetours = new List<Vector3>();
	void Connecting()
	{
		if(lastInteractState!=InteractState.Connecting)
		{
			TempDetours = new List<Vector3>();
		}

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
					interactState = InteractState.Finding;
					return;
				}
			}

			TempDetours.Add(projectedPosition);
		}

		
	}
}
