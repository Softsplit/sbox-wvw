public sealed class TestOutputNode : Node
{
	[Property] public NodeOutput NodeOutput {get;set;}
	protected override void OnFixedUpdate()
	{
		if(Input.Pressed("use"))
			Output();
	}
}
