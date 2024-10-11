namespace Sandbox;

[Title( "Vector Line Renderer" ), Category( "Rendering" ), Icon( "show_chart" )]
public sealed class VectorLineRenderer : Component, Component.ExecuteInEditor
{
	[Property] public bool RunBySelf { get; set; } = true;

	[Group( "Points" ), Property] public List<Vector3> Points { get; set; }

	[Group( "Appearance" ), Property] public float Noise { get; set; } = 1f;

	[Group( "Appearance" ), Property] public Gradient Color { get; set; } = global::Color.Cyan;

	[Group( "Appearance" ), Property] public Curve Width { get; set; } = 5f;

	[Group( "Spline" ), Property, Range( 1f, 32f, 0.01f, true, true )] public int SplineInterpolation { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f, 0.01f, true, true )] public float SplineTension { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f, 0.01f, true, true )] public float SplineContinuity { get; set; }

	[Group( "Spline" ), Property, Range( -1f, 1f, 0.01f, true, true )] public float SplineBias { get; set; }

	[Group( "End Caps" ), Property] public SceneLineObject.CapStyle StartCap { get; set; }

	[Group( "End Caps" ), Property] public SceneLineObject.CapStyle EndCap { get; set; }

	[Group( "Rendering" ), Property] public bool IsWireframe { get; set; }

	[Group( "Rendering" ), Property] public bool IsOpaque { get; set; } = true;

	private SceneLineObject _lineObject;
	private bool _hasRun;

	protected override void OnEnabled()
	{
		_lineObject = new SceneLineObject( Scene.SceneWorld )
		{
			Transform = Transform.World
		};
	}

	protected override void OnDisabled()
	{
		_lineObject?.Delete();
		_lineObject = null;
	}

	protected override void OnPreRender()
	{
		if ( RunBySelf )
		{
			ExecuteRendering();
		}
		else
		{
			_lineObject.RenderingEnabled = _hasRun;
		}

		_hasRun = false;
	}

	public void ExecuteRendering()
	{
		_hasRun = true;

		if ( _lineObject == null ) return;

		if ( Points == null )
		{
			_lineObject.RenderingEnabled = false;
			return;
		}

		var pointsWithNoise = Points.Select( ( point, index ) =>
			index == 0 || index == Points.Count - 1 ? point : point + (Vector3.Random * Noise) );

		int pointCount = pointsWithNoise.Count();
		if ( pointCount <= 1 )
		{
			_lineObject.RenderingEnabled = false;
			return;
		}

		ConfigureLineObject();

		if ( pointCount == 2 || SplineInterpolation == 1 )
		{
			DrawStraightLines( pointsWithNoise, pointCount );
		}
		else
		{
			DrawSplineLines( pointsWithNoise, pointCount );
		}

		_lineObject.EndLine();
	}

	private void ConfigureLineObject()
	{
		_lineObject.StartCap = StartCap;
		_lineObject.EndCap = EndCap;
		_lineObject.Wireframe = IsWireframe;
		_lineObject.Opaque = IsOpaque;
		_lineObject.RenderingEnabled = true;
		_lineObject.Transform = Transform.World;
		_lineObject.Flags.CastShadows = true;

		var attributes = _lineObject.Attributes;
		attributes.Set( StringToken.Literal( "BaseTexture", 388050857u ), Texture.White, -1 );
		attributes.SetCombo( StringToken.Literal( "D_BLEND", 348860154u ), !IsOpaque ? 1 : 0 );
	}

	private void DrawStraightLines( IEnumerable<Vector3> points, int pointCount )
	{
		int index = 0;
		foreach ( var point in points )
		{
			float time = (float)index / pointCount;
			_lineObject.AddLinePoint( point, Color.Evaluate( time ), Width.Evaluate( time ) );
			index++;
		}
	}

	private void DrawSplineLines( IEnumerable<Vector3> points, int pointCount )
	{
		int index = 0;
		int interpolatedPoints = (pointCount - 1) * SplineInterpolation.Clamp( 1, 100 );

		foreach ( var point in points.TcbSpline( SplineInterpolation, SplineTension, SplineContinuity, SplineBias ) )
		{
			float time = (float)index / interpolatedPoints;
			_lineObject.AddLinePoint( point, Color.Evaluate( time ), Width.Evaluate( time ) );
			index++;
		}
	}
}
