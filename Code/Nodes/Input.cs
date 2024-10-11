public sealed class NodeInput : Component
{
	protected override void OnStart()
	{
		node = Components.GetInParent<Node>();
	}
	[Property] public int index {get;set;}
	[Property] public Node node {get;set;}
	[Property] public NodeOutput.OutputType AcceptedType {get;set;} = NodeOutput.OutputType.Normal;
}
