using System;
using System.Threading.Tasks;
using Scene = Sandbox.Scene;

namespace Softsplit;

public static partial class GameObjectExtensions
{
	public static async void DestroyAsync(this GameObject src, float time, bool withChildren = true)
	{
		await Task.Delay((int)MathF.Round(time*1000f));
		while(!withChildren && src.Children.Count > 0)
		{
			src.Children[0].SetParent(src.Parent);
		}
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
