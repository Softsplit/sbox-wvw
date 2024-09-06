using System;
using System.Threading.Tasks;
using Scene = Sandbox.Scene;

namespace Softsplit;

public static partial class GameObjectExtensions
{
	public static async void DestroyAsync(this GameObject src, float time)
	{
		await Task.Delay((int)MathF.Round(time*1000f));
		src.Destroy();
	}
	public static void CopyPropertiesTo( this Component src, Component dst )
	{
		var json = src.Serialize().AsObject();
		json.Remove( "__guid" );
		dst.DeserializeImmediately( json );
	}

	public static string GetScenePath( this GameObject go )
	{
		return go is Scene ? "" : $"{go.Parent.GetScenePath()}/{go.Name}";
	}
}
