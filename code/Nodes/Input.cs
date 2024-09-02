using Sandbox;

public sealed class NodeInput : Component
{
	protected override void OnStart()
	{
		node = Components.GetInParent<Node>();
	}
	[Property] public Node node {get;set;}
}
