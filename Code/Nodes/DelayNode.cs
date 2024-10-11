using Sandbox;

public sealed class DelayNode : Node
{
	[Property] public NumberInput NumberInput {get;set;}
	public override async void Tick( int index )
	{
		await Task.DelaySeconds(NumberInput.Value);
		Output();
	}
}
