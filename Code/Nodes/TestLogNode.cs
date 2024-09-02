public sealed class TestLogNode : Component
{
	[Property] public NodeInput NodeInput { get; set; }
	[Property] public string LogText { get; set; } = "Hello world!";

	protected override void OnUpdate()
	{
		if ( NodeInput.Value )
		{
			Log.Info( LogText );
			NodeInput.Value = false;
		}
	}
}
