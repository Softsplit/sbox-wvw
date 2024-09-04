public sealed class NodeOutput : Component
{
	[Property] public List<NodeInput> ConnectedInputs {get;set;} = new List<NodeInput>();
	[Property] public Color NeutralColour {get;set;} = Color.White;
	[Property] public Color SendColour {get;set;} = Color.Green;
	[Property] public OutputType outputType {get;set;} = OutputType.Normal;

	public const float VisualSendTime = 0.2f;

    public enum OutputType
    {
        Normal,
        Number,
        Mana
    }

    float SendTime;
    public void DrawLineTo(Vector3 pos)
    {
        Vector3 startPoint = Transform.Position;
        Gizmo.Draw.Color = Time.Now-SendTime < VisualSendTime ? SendColour : NeutralColour;
        Gizmo.Draw.Line(startPoint, pos);
    }
	protected override void OnPreRender()
	{
		for(int i = 0; i < ConnectedInputs.Count; i++)
        {
            if(ConnectedInputs[i].IsValid())
                DrawLineTo(ConnectedInputs[i].Transform.Position);
        }
	}
	

	public void SendSignal()
    {
        SendTime = Time.Now;
        foreach(NodeInput nodeInput in ConnectedInputs)
        {
            if(nodeInput.AcceptedType != OutputType.Normal) break;
            nodeInput.node.Tick(nodeInput.index);
        }
    }

    public void SendNumberSignal(float number)
    {
        SendTime = Time.Now;
        foreach(NodeInput nodeInput in ConnectedInputs)
        {
            if(nodeInput.AcceptedType != OutputType.Number) break;
            nodeInput.node.NumberTick(nodeInput.index, number);
        }
    }

    public void SendMana(Node node, float number)
    {
        SendTime = Time.Now;
        foreach(NodeInput nodeInput in ConnectedInputs)
        {
            if(nodeInput.AcceptedType != OutputType.Mana) break;
            nodeInput.node.AddMana(node, number);
        }
    }


}