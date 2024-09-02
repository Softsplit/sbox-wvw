using Sandbox;

public sealed class TestOutputNode : Component
{
	[Property] public NodeOutput NodeOutput {get;set;}
	protected override void OnFixedUpdate()
	{
		if(Input.Pressed("use"))
			NodeOutput.Send();
	}
}
