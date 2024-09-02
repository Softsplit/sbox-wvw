using Sandbox;

public sealed class SpellMaker : Component
{
	[Property] GameObject DraggedNode;
	Vector3 MouseDragOrigin;
	Vector3 NodeDragOrigin;
	float DragDis;
	[Property] InteractState interactState;

	enum InteractState
	{
		Finding,
		Dragging,
		Connecting
	}
	SceneTraceResult mouseRay;
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
	}

	void Finding()
	{
		if(!Input.Down("attack1")) return;

		if(!mouseRay.Hit) return;

		if(mouseRay.GameObject.Tags.Contains("node"))
		{
			DraggedNode = mouseRay.GameObject;
			
			Vector3 cameraForward = Scene.Camera.Transform.World.Forward;
			Vector3 cameraPosition = Scene.Camera.Transform.Position;
			Vector3 nodePosition = DraggedNode.Transform.LocalPosition;

			DragDis = Vector3.Dot(nodePosition - cameraPosition, cameraForward);

			MouseDragOrigin = nodePosition;

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
	void Connecting()
	{

	}
}
