using Sandbox;

public sealed class NotEqualsNode : Node
{
	float a;
	float b;
	public override void NumberTick(int index, float number)
	{
		if(index == 0) a = number;
		else b = number;
	}

	protected override void OnFixedUpdate()
	{
		if(a != b)
			Output();
		a = 0;
		b = 0;
	}
}
