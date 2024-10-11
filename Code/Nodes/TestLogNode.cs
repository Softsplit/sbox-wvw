public sealed class TestLogNode : Node
{
	[Property] public string LogText {get;set;} = "sex";
	public override void Tick(int index)
	{
		Log.Info(LogText);
	}
}
