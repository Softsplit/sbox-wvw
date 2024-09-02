using Sandbox;

public sealed class TestLogNode : Node
{
	[Property] public string LogText {get;set;} = "sex";
	public override void Tick()
	{
		Log.Info(LogText);
	}
}
