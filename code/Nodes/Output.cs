public sealed class NodeOutput : Component
{
	[Property] public List<NodeInput> ConnectedInputs {get;set;} = new List<NodeInput>();
	[Property] public List<List<Vector3>> PathDetours {get;set;} = new List<List<Vector3>>();
	[Property] public Color NeutralColour {get;set;} = Color.White;
	[Property] public Color SendColour {get;set;} = Color.Green;
	[Property] public OutputType outputType {get;set;} = OutputType.Normal;

    public const float VisualSendTime = 0.2f;

    public enum OutputType
    {
        Normal,
        Number
    }

    float SendTime;
    public void DrawLineTo(Vector3 pos, List<Vector3> Detours)
    {
        Vector3 startPoint = Transform.Position;
        Gizmo.Draw.Color = Time.Now-SendTime < VisualSendTime ? SendColour : NeutralColour;
        foreach (var detour in Detours)
        {
            Gizmo.Draw.Line(startPoint, detour);
            startPoint = detour;
        }

        Gizmo.Draw.Line(startPoint, pos);
    }
	protected override void OnUpdate()
	{
		for(int i = 0; i < ConnectedInputs.Count && i < PathDetours.Count; i++)
        {
            DrawLineTo(ConnectedInputs[i].Transform.Position, PathDetours[i]);
        }
	}
	

	public void SendSignal()
    {
        SendTime = Time.Now;
        foreach(NodeInput nodeInput in ConnectedInputs)
        {
            nodeInput.node.Tick(nodeInput.index);
        }
    }

    public void SendNumberSignal(float number)
    {
        SendTime = Time.Now;
        foreach(NodeInput nodeInput in ConnectedInputs)
        {
            nodeInput.node.NumberTick(nodeInput.index, number);
        }
    }


}