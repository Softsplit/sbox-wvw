public sealed class TestOutputNode : Node
{
	[Property] public NodeOutput NodeOutput {get;set;}
	[Property] public string Key {get;set;} = "Use";
	protected override void OnFixedUpdate()
	{
		if(Input.Down(Key))
			Output();
	}
}
