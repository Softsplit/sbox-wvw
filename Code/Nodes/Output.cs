public sealed class NodeOutput : Component
{
	[Property] public List<Connection> Connections {get;set;} = new List<Connection>();
	[Property] public Color NeutralColour {get;set;} = Color.White;
	[Property] public Color SendColour {get;set;} = Color.Green;
	[Property] public OutputType outputType {get;set;} = OutputType.Normal;

	public const float VisualSendTime = 0.2f;

    public class Connection
    {
        [Property] public GameObject ConnectedObject {get;set;}
        [Property] public int Index {get;set;}

        Node _node;
        public Node ConnectedNode
        {
            get
            {
                if(_node == null)
                    _node = ConnectedObject.Components.Get<Node>();

                return _node;
            }
            set
            {
                _node = value;
            }
        }

        public NodeInput NodeInput()
        {
            if(!ConnectedNode.IsValid()) return null;
            if(ConnectedNode.Inputs.Count < Index) return null;
            return ConnectedNode.Inputs[Index];
        }
    }

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
		for(int i = 0; i < Connections.Count; i++)
        {
            NodeInput nodeInput = Connections[i].NodeInput();
            if(nodeInput.IsValid())
                DrawLineTo(nodeInput.Transform.Position);
        }
	}
	

	public void SendSignal()
    {
        SendTime = Time.Now;
        foreach(Connection connection in Connections)
        {
            NodeInput nodeInput = connection.NodeInput();
            if(nodeInput.AcceptedType != OutputType.Normal) break;
            nodeInput.node.Tick(nodeInput.index);
        }
    }

    public void SendNumberSignal(float number)
    {
        SendTime = Time.Now;
        foreach(Connection connection in Connections)
        {
            NodeInput nodeInput = connection.NodeInput();
            if(nodeInput.AcceptedType != OutputType.Number) break;
            nodeInput.node.NumberTick(nodeInput.index, number);
        }
    }

    public void SendMana(Node node, float number)
    {
        SendTime = Time.Now;
        foreach(Connection connection in Connections)
        {
            NodeInput nodeInput = connection.NodeInput();
            if(nodeInput.AcceptedType != OutputType.Mana) break;
            nodeInput.node.AddMana(node, number);
        }
    }


}