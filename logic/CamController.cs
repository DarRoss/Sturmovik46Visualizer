using Godot;

public partial class CamController : Node3D
{
	private const float MOUSE_SENSE = 0.05f;
	private const int MAX_CAM_PITCH = 89;
	private const int MAX_SPEED = 5000;
	private const int MIN_SPEED = 1;
	private const float SPD_INC_MULT = 1.2f;

	private Node3D camYaw;
	private Node3D camPitch;
	private Node3D camFp;

	private Vector3 movementDelta = Vector3.Zero;
	// x-axis denotes pitch rads, y-axis denotes yaw rads
	private Vector2 camAngleBuffer = Vector2.Zero;

	private float moveSpeed = (MAX_SPEED + MIN_SPEED) / 2;

	public override void _Ready()
	{
		camYaw = GetNode<Node3D>("CamYaw");
		camPitch = GetNode<Node3D>("CamYaw/CamPitch");
		camFp = GetNode<Camera3D>("CamYaw/CamPitch/CamFp");
	}
	
	public override void _Input(InputEvent ie)
    {
        if(Input.GetMouseMode() == Input.MouseModeEnum.Captured && ie is InputEventMouseMotion iemm)
        {
            camAngleBuffer.X += Mathf.DegToRad(-iemm.Relative.Y * MOUSE_SENSE);
            camAngleBuffer.Y += Mathf.DegToRad(-iemm.Relative.X * MOUSE_SENSE);
        }
		if(ie.IsAction("Forward") || ie.IsAction("Backward"))
		{
			movementDelta.Z = -(Input.GetActionStrength("Forward") - Input.GetActionStrength("Backward"));
		}
		if(ie.IsAction("Right") || ie.IsAction("Left"))
		{
			movementDelta.X = Input.GetActionStrength("Right") - Input.GetActionStrength("Left");
		}
		if(ie.IsAction("Up") || ie.IsAction("Down"))
		{
			movementDelta.Y = Input.GetActionStrength("Up") - Input.GetActionStrength("Down");
		}
		if(ie.IsActionReleased("IncreaseSpeed"))
		{
			moveSpeed *= SPD_INC_MULT;
			moveSpeed = Mathf.Min(moveSpeed, MAX_SPEED);
		}
		if(ie.IsActionReleased("DecreaseSpeed"))
		{
			moveSpeed /= SPD_INC_MULT;
			moveSpeed = Mathf.Max(moveSpeed, MIN_SPEED);
		}
    }

	public override void _PhysicsProcess(double delta)
	{
		if(!Mathf.IsZeroApprox(camAngleBuffer.X))
		{
			camPitch.RotateX(camAngleBuffer.X);
			camAngleBuffer.X = 0;
            float maxPitch = Mathf.DegToRad(MAX_CAM_PITCH);
			camPitch.Rotation = new Vector3(Mathf.Clamp(camPitch.Rotation.X, -maxPitch, maxPitch), 0, 0);
		}
		if(!Mathf.IsZeroApprox(camAngleBuffer.Y))
		{
			camYaw.RotateY(camAngleBuffer.Y);
			camAngleBuffer.Y = 0;
		}
		if(!movementDelta.IsZeroApprox())
		{
			Vector3 rotatedMovement = movementDelta.Rotated(Vector3.Right, camPitch.Rotation.X)
				.Rotated(Vector3.Up, camYaw.Rotation.Y);
			GlobalPosition += rotatedMovement * moveSpeed * (float)delta;
		}
	}
}
