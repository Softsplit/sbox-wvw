public sealed class NodeOutput : Component
{
	[Property] public List<NodeInput> ConnectedInputs { get; set; } = new List<NodeInput>();
	[Property] public VectorLineRenderer LineRenderer { get; set; }
	[Property] public List<List<Vector3>> PathDetours { get; set; } = new List<List<Vector3>>();

	protected override void OnStart()
	{
		LineRenderer = Components.GetOrCreate<VectorLineRenderer>();
		LineRenderer.Width = 0.01f;
		LineRenderer.Noise = 0;
		LineRenderer.RunBySelf = false;
	}

	public void DrawLineTo( Vector3 targetPosition, List<Vector3> detours )
	{
		Vector3 startPoint = Transform.Position;

		foreach ( var detour in detours )
		{
			Gizmo.Draw.Line( startPoint, detour );
			startPoint = detour;
		}

		Gizmo.Draw.Line( startPoint, targetPosition );
	}

	protected override void OnUpdate()
	{
		for ( int i = 0; i < ConnectedInputs.Count && i < PathDetours.Count; i++ )
		{
			DrawLineTo( ConnectedInputs[i].Transform.Position, PathDetours[i] );
		}
	}

	public void SendSignal()
	{
		foreach ( var input in ConnectedInputs )
		{
			input.Value = true;
		}
	}
}
