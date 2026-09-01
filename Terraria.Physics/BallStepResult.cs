namespace Terraria.Physics;

public struct BallStepResult(BallState state)
{
	public readonly BallState State = state;

	public static BallStepResult OutOfBounds()
	{
		return new BallStepResult(BallState.OutOfBounds);
	}

	public static BallStepResult Moving()
	{
		return new BallStepResult(BallState.Moving);
	}

	public static BallStepResult Resting()
	{
		return new BallStepResult(BallState.Resting);
	}
}
