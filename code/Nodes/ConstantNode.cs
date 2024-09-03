using Sandbox;

public sealed class ConstantNode : Node
{
	[Property] public NumberInput NumberInput {get;set;}
	protected override void OnFixedUpdate()
	{
		NumberOutput(NumberInput.Value);
	}
}
