using Godot;
using System;
using System.Diagnostics;
using System.Globalization;

public partial class PlayerController : Node2D
{
	//important info for the player.
    public float grSpeed;
    public float grAngle;

	private bool _grounded;
    public bool _Grounded 
	{
		get { return _grounded; }
		set 
		{
			_grounded = value;

			if (value)
			{

			}
			else
			{
				_turn = false;
			}

			if (face != null)
			{
				face.circular = value;
			}
		}
	}

    public Vector2 velocity;
    public bool sprteRot = false;

	//input values.
    public float inputHorizontal;
    public float inputVertical;
    public float inputJump;
    public float inputAction1;
    public float inputAction2;

	//history of inputs, used for input combos.
	public byte[] inputHistory;
	public float[] inputHistoryTimes;

	//general states of the player.
	//also note that any public boolean starting with _ will be added to debug states list.
    public int _index; //random states.
	public float _timer;
    public bool _attacking;
    public bool _invincible;
    public bool _jump;
    public bool _edge;
    public bool _wall;
    public bool _crouch;
	public bool _turn;
    public bool _ignoreRecoil; //ignores recoil while attacking.

	//states useful for things like cutscenes or attack cooldowns.
	public byte _noFlip = 1; //0 = dont flip sprite; default: 1
    public bool _noJump = false; //true = cannot jump. default: false.
    public byte _noMove = 1; //0 = left or right ignored but still readable; default: 1
    public byte _noInput = 1; //0 = ignores gameplay input entirely; default: 1
    public byte _noSpeed = 1; //0 = speed entirely stopped; default: 1

	//visual-related values.
	public Vector2 bodyOffset = new Vector2(0, 6);
	public int facing;

	//etc.
	float grAngleDeg;
	Vector2 grPerpen;
	int signAngle;
	float airRot;
	float fixedDelta;
	Vector2 lastPos;


	[ExportSubgroup("Vars")]

	//timers are in ticks. The game's tickspeed is 100.
	public float neutralTimer;
	public float neutralTimerReset = .3f;

	//determines speed/strength of forces.
    public float acceleration; //acceleration.
    public float deceleration; //deceleration.
    public float runSpeed; //top running speed.
    public float capSpeed; //max capped speed.
    public float friction; //friction.
    public float airAcceleration; //air acceleration.
    public float jumpForce; //jump force.
	public float gravity; //gravity.

	//stat versions are the defaults. dont change unless for permanent changes, like with upgrades.
	[Export] public float statAcceleration = 7.2f;
	[Export] public float statDeceleration = 48f;
	[Export] public float statRunSpeed = 130f;
	[Export] public float statCapSpeed = 720f;
	[Export] public float statFriction = 4.8f;
	[Export] public float statAirAcceleration = 6f;
	[Export] public float statJumpForce = 225f;
	[Export] public float statGravity = 4.8f;


	[ExportSubgroup("References")]

	Node2D smooth;
	public Sprite2D sprite;
	public PlayerAnimator animator;
	public PlayerTailAnimator tailAnimator;
	[Export] public PlayerFace face;
	[Export] public ClassBase playerClass;
	CameraHandler cameraHandler;


	[ExportSubgroup("Sensors")]


	RaycastHit2D SensGrMain;
	RaycastHit2D SensGrRight;
	RaycastHit2D SensGrLeft;

	float SensGrRightDis;
	float SensGrLeftDis;
	short SensGrDir;

	RaycastHit2D SensWallR;
	RaycastHit2D SensWallL;

	RaycastHit2D SensCeilMain;
	RaycastHit2D SensCeilRight;
	RaycastHit2D SensCeilLeft;

	RaycastHit2D SensSpeed;

	public uint sensMask = 0b01;


	delegate void PlayerState();
	PlayerState State;


	public override void _Ready()
	{
		acceleration = statAcceleration; deceleration = statDeceleration; runSpeed = statRunSpeed; capSpeed = statCapSpeed;	
		friction = statFriction; airAcceleration = statAirAcceleration; jumpForce = statJumpForce; gravity = statGravity;

		spaceState = GetWorld2D().DirectSpaceState;
		query = PhysicsRayQueryParameters2D.Create(Vector2.Zero, Vector2.Up, sensMask);

		sprite = GetNode<Sprite2D>("Visuals/Animator/Sprite");
		animator = GetNode<PlayerAnimator>("Visuals/Animator");
		cameraHandler = GetNode<CameraHandler>("Camera");
		smooth = GetNode<Node2D>("Visuals");

		animator.player = this;
		animator.tailAnimator = tailAnimator;

		tailAnimator.sprite = sprite;

		ClearInput();

        RemoveChild(cameraHandler);
		CallDeferred("SetChild", cameraHandler);

		State = MoveAir;
	}

#if DEBUG

	[ExportSubgroup("Debug")]

	public DrawRay[] drawRays = new DrawRay[10];
	byte drawTimes;


	public override void _Draw()
	{
		if (Engine.IsEditorHint()){
			sprite.Position = Position;
		}
	}

#endif

    public override void _Process(double delta)
    {
		float deltaF = (float)delta;
		sprite.Position = smooth.Position;

		TimerInput(deltaF);

        if (!sprteRot)
        {
            //sprite.Rotate(delt * Mathf.DegToRad(airRot) * Mathf.Sign(Convert.ToByte(sprite.FlipH) - .5f));
            return;
        }

        if (_Grounded)
        {
            sprite.GlobalRotation = Mathf.LerpAngle(sprite.GlobalRotation, Rotation + grAngle, deltaF*Mathf.Clamp(Mathf.Abs(grSpeed)/12, 17, 100));
        }
        else
        {
            float dirRot = velocity.Angle() * airRot;

            //anim.transform.eulerAngles = new Vector3(0, 0, airRot);
            sprite.GlobalRotation = Mathf.LerpAngle(sprite.Rotation, dirRot, deltaF*100);
        }
    }

    public override void _PhysicsProcess(double delta)
	{
		fixedDelta = (float)delta;

		if (_timer > 0) _timer -= fixedDelta;
		else _timer = 0;

		#if DEBUG	
		drawRays = new DrawRay[10];
		drawTimes = 0;
		#endif

		PhysicsUpdate();
		
        SpeedCast();

		SensWall();

		State();

		QueueRedraw();
	}

	void PhysicsUpdate()
	{
		lastPos = Position;
		Position += velocity*fixedDelta;
	}

#region Move

#region Ground

	void MoveGround()
	{
		SensGr();

        MvGrInput();

		//Check if still on the ground.
		if (_Grounded)
        {
            grSpeed = Mathf.Clamp(grSpeed, -runSpeed, runSpeed);// * spNull;
            velocity = grSpeed * grPerpen;

			//Check if its turning currently.
			if (_turn && _timer == 0)
			{
				_turn = false;
			}
        }

		bodyOffset = Vector2.Down * 6;
	}

	void MvGrInput()
	{
		if (inputHorizontal * _noMove >= 0.4)
		{
			if (grSpeed <= 0)
			{
				//turn around on a stand-still.
				if (facing == 1)
				{
					grSpeed += acceleration;
				} 
				else
				{
					grSpeed += deceleration;

					_turn = true;
					_timer = 0.18f;
				}
			}
			else if (grSpeed < runSpeed)
			{
				grSpeed += acceleration;
				
				if (grSpeed >= runSpeed)
				{
					grSpeed = runSpeed;
				}
			}

			facing = 1;
			face.facing = 1;
			
			face.lookDirection = Vector2.Right * inputHorizontal;
			sprite.FlipH = Convert.ToBoolean((Mathf.Sign(grSpeed + inputHorizontal * 0.1f) + 1) * (_noFlip+animator.flipNullifierAnim) + Convert.ToByte(sprite.FlipH) * (1 - (_noFlip+animator.flipNullifierAnim)));
		}
		else if (inputHorizontal * _noMove <= -0.4f)
		{
			if (grSpeed >= 0)
			{
				//turn around on a stand-still.
				if (facing == -1)
				{
					grSpeed -= acceleration;
				} 
				else
				{
					grSpeed -= deceleration;

					_turn = true;
					_timer = 0.18f;
				}
			}
			else if (grSpeed > -runSpeed)
			{
				grSpeed -= acceleration;
				
				if (grSpeed <= -runSpeed)
				{
					grSpeed = -runSpeed;
				}
			}

			facing = -1;
			face.facing = -1;
			
			face.lookDirection = Vector2.Right * inputHorizontal;
			sprite.FlipH = Convert.ToBoolean((Mathf.Sign(grSpeed + inputHorizontal * 0.1f) + 1) * (_noFlip+animator.flipNullifierAnim) + Convert.ToByte(sprite.FlipH) * (1 - (_noFlip+animator.flipNullifierAnim)));
		}
		else
		{
			grSpeed -= Mathf.Min(Mathf.Abs(grSpeed), friction) * Mathf.Sign(grSpeed);
			
			_turn = false;
		}
	}

	#endregion

#region Air

	void MoveAir()
	{
        MvAirInput();

        //anim.SetFloat("ySpeed", vel.Y);

        velocity.Y += gravity;

        if (velocity.Y > -1)
        {
            SensGrRight = PushRaycast(Position + Transform.X * 8, Transform.Y*14, Colors.Red);
            SensGrLeft = PushRaycast(Position - Transform.X * 8, Transform.Y*14, Colors.Red);

            if (SensGrLeft.hit || SensGrRight.hit)
            {
        		SensGrCheckLite();
        		GetTileAngle();

                LandOnGround();
            }
        }
        else SensCeil();

		face.lookDirection = velocity.Normalized();
		bodyOffset = Vector2.Down * 3;
		
		if (inputHorizontal == 0) return;
		
		facing = Mathf.Sign(inputHorizontal);
		face.facing = facing;
	}

	void MvAirInput()
    {
        if (Mathf.Abs(velocity.X) > runSpeed)
        {
           	if (inputHorizontal != 0)
           	{
               	velocity += new Vector2(Convert.ToByte(inputHorizontal != Mathf.Sign(velocity.X)) * Mathf.Sign(inputHorizontal) * airAcceleration, 0);
           	}
        }
        else
        {
           	velocity = new Vector2(Mathf.Clamp(velocity.X + airAcceleration * inputHorizontal * _noInput * _noMove, -runSpeed, runSpeed), velocity.Y);
        }
    }

#endregion

#endregion

#region Collision

#region Ground

	void SensGr()
	{
		SensGrRightScan();
		SensGrLeftScan();

		SensGrCheck();
	}

	void SensGrRightScan()
	{
		SensGrRight = PushRaycast(Position+Transform.X*8, Transform.Y*16, Colors.Green);
		SensGrRightDis = SensGrRight.distance;
	}
	void SensGrLeftScan()
	{
		SensGrLeft = PushRaycast(Position-Transform.X*8, Transform.Y*16, Colors.Green);
		SensGrLeftDis = SensGrLeft.distance;
	}

	void SensGrCheck()
	{
		if (SensGrRight.hit == SensGrLeft.hit)
		{
			_edge = false;

			if (SensGrRight.hit)
			{
				if (SensGrLeft.distance >= SensGrRight.distance)
				{
					SensGrMain = SensGrRight;
					SensGrDir = 1;
				} else
				{
					SensGrMain = SensGrLeft;
					SensGrDir = -1;
				}

				SensGrReact();
			} else
			{
				FallPlayer();

				GD.Print("falling: none detected.");
			}
		} else
		{
			_edge = true;

			if (SensGrRight.hit)
			{
				SensGrMain = SensGrRight;
				SensGrDir = 1;
			} else
			{
				SensGrMain = SensGrLeft;
				SensGrDir = -1;
			}

			SensGrReact();
		}
	}

	void SensGrCheckLite()
	{
		if (SensGrRight.hit == SensGrLeft.hit)
		{
			if (SensGrLeft.distance >= SensGrRight.distance)
			{
				SensGrMain = SensGrRight;
				SensGrDir = 1;
			} else
			{
				SensGrMain = SensGrLeft;
				SensGrDir = -1;
			}

			_edge = false;
		} else
		{
			if (SensGrRight.hit)
			{
				SensGrMain = SensGrRight;
				SensGrDir = 1;
			} else
			{
				SensGrMain = SensGrLeft;
				SensGrDir = -1;
			}

			_edge = true;
		}
	}

	void SensGrReact()
	{
		GetTileAngle();

		Vector2 Tup = -Transform.Y * 12;
		if (_edge)
		{
			if (SensGrDir == Mathf.Sign(grAngleDeg)){
				Tup += Transform.Y*Mathf.Sin(Mathf.Abs(grAngle))*8;
			}
		}

		Position = new Vector2(Position.X, SensGrMain.pos.Y)+Tup;
	}

#endregion

#region Wall

    void SensWall()
    {
        SensWallScan();

        SensWallReact();
    }

	void SensWallScan()
	{
		SensWallL = PushRaycast(Position, -Transform.X * 8, Colors.Red);
		SensWallR = PushRaycast(Position, Transform.X * 8, Colors.Red);
	}

	void SensWallReact()
	{
		int amount = Convert.ToByte(SensWallL.hit) * 2 + Convert.ToByte(SensWallR.hit)*3;
        if (amount != 0)
        {
			_wall = true;

			RaycastHit2D rayWall;
			if (amount > 2) rayWall = SensWallR;
			else rayWall = SensWallL;

			float angle = Mathf.Abs(Mathf.RadToDeg(rayWall.norm.AngleTo(Vector2.Up)));
			//GD.Print("wall hit? " + amount + " " + rayWall.distance);
            if (angle > 45)
            {
                Position -= (8.1f - rayWall.distance) * Mathf.Sign(velocity.X) * Transform.X;

                float speedAngleMult = Mathf.Clamp(Mathf.Ceil(Mathf.Abs(grAngleDeg / 60)), 0, 1);
                grSpeed *= speedAngleMult;
                velocity.X *= speedAngleMult;

                RaycastHit2D rayL = PushRaycast(Position + Vector2.Left * 7, Vector2.Down * 2, Colors.Purple);
                RaycastHit2D rayR = PushRaycast(Position + Vector2.Right * 7, Vector2.Down * 2, Colors.Purple);
                if (Convert.ToByte(rayL.hit) + Convert.ToByte(rayR.hit) == 0)
                {
                    _Grounded = false;
					State = MoveAir;
                    FallToGround();
                }
            }
        }
	
	}

#endregion

#region Ceiling

	void SensCeil()
	{
		SensCeilScan();

		SensCeilChoose();

		SensCeilReact();
	}

    void SensCeilScan()
    {
        SensCeilRight = PushRaycast(Position + Transform.X * 8, -Transform.Y * 16, Colors.Yellow);
        SensCeilLeft = PushRaycast(Position - Transform.X * 8, -Transform.Y * 16, Colors.Blue);
    }

    void SensCeilChoose()
    {
        if (SensCeilLeft.distance >= SensCeilRight.distance)
        {
            if (SensCeilRight.hit)
            {
                SensCeilMain = SensCeilRight;
            }
            else
            {
                SensCeilMain = SensCeilLeft;
            }
        }
        else
        {
            SensCeilMain = SensCeilRight;
        }
    }

    void SensCeilReact()
    {
        if (SensCeilMain.hit)
        {
            SensCeilGetAngle();
        }
    }

    void SensCeilGetAngle()
    {
		grPerpen = -SensCeilMain.norm.Orthogonal();

        float rot = Mathf.RadToDeg(SensCeilMain.norm.AngleTo(Transform.Y));
        float signRot = Mathf.Sign(rot);

        //if >70 angle, turn to 90 for smooth velocity transition.
        if (Mathf.Abs(rot) > 65000)
        {
            //rot = 90 * signRot;
        }

        if (Mathf.Abs(Mathf.Round(rot)) >= 45000)
        {
			/*Vector2 velOld = vel;

            RotationDegrees = 90 * signRot;

			SensGrRight = SensCeilRight;
			SensGrLeft = SensCeilLeft;
			SensGrMain = SensCeilMain;

			GetTileAngle();
            LandOnGround();

            grAngleDeg = rot - RotationDegrees;
			grAngle = Mathf.DegToRad(grAngleDeg);
			
			float sp = rot / 90;
            grSpeed = velOld.X * (1 - sp) - velOld.Y * sp * signAngle;

            side = true;
            down = false;
            up = false;
            rev = Mathf.RoundToInt(signRot);
            sprte.FlipH = Convert.ToBoolean(Mathf.Sign(grSpeed) + 1);

            Vector2 Tup = Transform.Y * -16;
            Position = new Vector2(SensCeilMain.pos.X, Position.Y)+Tup;*/
        }
        else
        {
            float CeilRange = SensCeilMain.distance;
            
            float clampRot = Mathf.Clamp(Mathf.Round(grAngleDeg) * 100, 0, 1);

            RaycastHit2D sideRay = PushRaycast(Position - Transform.Y * CeilRange, Transform.X * signAngle * 8);
            Position += Transform.Y * (16 - CeilRange) + Transform.X * ((8 - sideRay.distance) / 2 * (clampRot * signAngle));
            velocity = new Vector2(velocity.X, clampRot);
        }
    }

#endregion

#region SpeedCast

	/// <summary>
	/// SpeedCast is a ray used to keep sonic out of walls, 
	/// basically just being a ray that checks last tick's and current position.
	/// </summary>
	
    void SpeedCast()
    {
        SpeedCastScan();

        SpeedCastReact();
    }

    void SpeedCastScan()
    {
        SensSpeed = PushRaycast(lastPos, Position - lastPos, Colors.White);
    }

    void SpeedCastReact()
    {
        if (SensSpeed.hit)
        {
            //RaycastHit2D SensSpeedcastLastRay = PushRaycast(lastPos, Position - lastPos, purple);
			GD.Print("SpeedCast Hit: " + Position);

            Position = SensSpeed.pos + SensSpeed.norm * 8;
        }
    }

#endregion

#endregion

#region Commands

#region Moves

	void ActionJump()
	{
    	float grAng = grAngle;
    	Vector2 dir = (-Transform.Y * Mathf.Cos(Mathf.Abs(grAng)) + Transform.X * Mathf.Sin(grAng)).Normalized() * jumpForce;

    	_Grounded = false;
		State = MoveAir;
    	SetRot(false, 0, 0);
    	FallToGround();

		animator.flipNullifierAnim = 0;
		tailAnimator.StartJump();

    	velocity += dir;

		animator.animationValues["grounded"] = 0;
		animator.SetAnimationState(0, 6); //JumpState.

    	_jump = true;
	}

#endregion

	void FallPlayer()
	{
        Vector2 oldVel = 16 * Mathf.Sign(grSpeed) * Transform.X;

        float x;
        float y;
        x = Mathf.Round((Position.X - oldVel.X)/16)*16;
        y = Mathf.Round((Position.Y - oldVel.Y)/16)*16;
        Vector2 curTile = new(x, y);

        Vector2 off = -Mathf.Sign(grSpeed) * Transform.X;
        RaycastHit2D ray = PushRaycast(curTile+off, Transform.Y * 17, Colors.Red);
		//normal is odd??

        Position = new Vector2(Position.X, ray.pos.Y - 16);

        Vector2 norm = -ray.norm.Orthogonal();
        velocity = grSpeed * norm;

        _Grounded = false;
		State = MoveAir;
        FallToGround();
	}

	public void FallToGround()
	{
		Rotation = 0;
		_edge = false;
		grAngle = 0;
		grAngleDeg = 0;
		_noInput = 1;
	}

	void LandOnGround()
	{
        _Grounded = true;
		State = MoveGround;
        _noInput = 1;
        _noMove = 1;

        _jump = false;
        SetRot(true);

		tailAnimator.EndJump();

		Vector2 velOld = velocity;

		//add physics from slopes.
        float ang = Mathf.Abs(grAngleDeg);

        float sp = ang / 90;
        grSpeed = velOld.X * (1 - sp) + velOld.Y * sp * signAngle;

        //animations.PlayAnim("RunningBlend");
        switch (_index)
        {
            case 0:
                //if (airRot == 1 && Mathf.Abs(grSpeed) > 8) anim.Play("PlyrLandingRoll");
                break;
            case 5:
                //if (Mathf.Abs(grSpeed) > 8) anim.Play("PlyrLandingRoll");
                break;
            case 7:
                //DropDashLand();
                break;
        }

        sprite.Rotation = (Rotation + grAngle) * Convert.ToByte(sprteRot);

		sprite.FlipH = Convert.ToBoolean((Mathf.Sign(grSpeed + inputHorizontal * 0.1f) + 1) * (_noFlip+animator.flipNullifierAnim) + Convert.ToByte(sprite.FlipH) * (1 - (_noFlip+animator.flipNullifierAnim)));

        velocity = grSpeed * grPerpen;
	}

	void GetTileAngle()
	{
		grPerpen = -SensGrMain.norm.Orthogonal();

		grAngle = -SensGrMain.norm.AngleTo(-Transform.Y);
		grAngleDeg = Mathf.RadToDeg(grAngle);
		signAngle = Mathf.Sign(grAngle);
	}
	
	void SetChild(Node car){
		GetParent().AddChild(car);
	}

	public void SetClass(ClassBase classNode)
	{
		playerClass = classNode;
		//animator.stateMachineClass = (AnimationNodeStateMachinePlayback)animator.Get("parameters/" + classNode.className + "/StateMachine/playback");

		classNode.OnPlayerConnect(this);
	}

	public void StateSetCooldown(bool noJumping, byte noMove, byte noInput, byte noSpeed)
	{
		_noJump = noJumping;
		_noMove = noMove;
		_noInput = noInput;
		_noSpeed = noSpeed;
	}

	void AddInputToHistory(byte num)
	{
		byte previousPosition = num;
		float previousTime = 0;

		neutralTimer = neutralTimerReset;

		for (int i = 0; i < inputHistory.Length; ++i)
		{
			byte temp = inputHistory[i];
			inputHistory[i] = previousPosition;
			previousPosition = temp;
			
			float tempTime = inputHistoryTimes[i];
			inputHistoryTimes[i] = previousTime;
			previousTime = tempTime;
		}
	}
	
	void ClearInput()
	{
		byte[] blank = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
		inputHistory = blank;

		float[] blankTimes = {500, 500, 500, 500, 500, 500, 500, 500, 500, 500};
		inputHistoryTimes = blankTimes;
	}

	void TimerInput(float delta)
	{
		inputHistoryTimes[0] += delta;

		neutralTimer -= delta;

		if (neutralTimer >= 0) return;

		if (inputHistory[0] == 0) return;

		inputHistoryTimes[0] = 1;

		AddInputToHistory(0);
	}

    public float SetRotToVel()
    {
        return velocity.Angle()+90;
    }
	public void SetRot(bool rot)
    {
        sprteRot = rot;
    }
    public void SetRot(bool rot, float sp)
    {
        sprteRot = rot;
        airRot = sp;
    }
    public void SetRot(bool rot, float sp, float trns)
    {
        sprteRot = rot;
        airRot = sp;
        sprite.RotationDegrees = trns;
    }

    PhysicsDirectSpaceState2D spaceState;
	PhysicsRayQueryParameters2D query;
	Godot.Collections.Dictionary tempRayDictionary;
	RaycastHit2D PushRaycast(Vector2 from, Vector2 to, Color color)
	{ 
		#if DEBUG
		drawRays[drawTimes] = new DrawRay{from = from, to = from+to, color = color, num = drawTimes};
		++drawTimes; 
		#endif

		return PushRaycast(from, to);
	}
	RaycastHit2D PushRaycast(Vector2 from, Vector2 to)
	{
		query.From = from;
		query.To = from+to;
		tempRayDictionary = spaceState.IntersectRay(query); //do raycast thingamajig

		RaycastHit2D rayHit;
		if (tempRayDictionary.Count == 0) //check if empty so no stinky errors
		{
			rayHit = new RaycastHit2D
			{
				hit = false,
				pos = Vector2.Zero,
				norm = Vector2.Zero,
				distance = 0
			};

			tempRayDictionary.Dispose();

			return rayHit;
		}

		rayHit = new RaycastHit2D
		{
			hit = true,
			pos = (Vector2)tempRayDictionary["position"],
			norm = (Vector2)tempRayDictionary["normal"],
			distance = ((Vector2)tempRayDictionary["position"] - Position).Length()
		};

		tempRayDictionary.Dispose();

		return rayHit; 
	}

#endregion

	void InputMove(bool pressed, float axis, float axisAlt)
	{
		int input = 5;

		inputHorizontal = axis;

		//snap tap. this tech costs 200 euro and a vac xddddd.
		if (axisAlt != 0)
		{
			if (pressed) inputHorizontal = axis;
			else inputHorizontal = axisAlt;

			//Adds inbetween point, ensuring no impossible inputs (down right -> down left);
			input = (Mathf.Sign(inputVertical) + 1) * 3 + 2;

			if (input != 5) AddInputToHistory((byte)input);
		} else
		if (!pressed)
		{
			if (inputVertical == 0) return;
		}

		//Translate input into singular input direction.
		input = Mathf.Sign(inputHorizontal) + (Mathf.Sign(inputVertical) + 1) * 3 + 2;

		if (input == 5 || input == inputHistory[0]) return;

		AddInputToHistory((byte)input);
	}

	void InputLook(bool pressed, float axis, float axisAlt)
	{
		int input = 5;

		inputVertical = axis;

		//snap tap. this tech costs 200 euro and a vac xddddd.
		if (axisAlt != 0)
		{
			if (pressed) inputVertical = axis;
			else inputVertical = axisAlt;

			//Adds inbetween point, ensuring no impossible inputs (down right -> up right);
			input = Mathf.Sign(inputHorizontal) + 5;

			if (input != 5) AddInputToHistory((byte)input);
		} else
		if (!pressed)
		{
			if (inputHorizontal == 0) return;
		}

		//Translate input into singular input direction.
		input = Mathf.Sign(inputHorizontal) + (Mathf.Sign(inputVertical)+1) * 3 + 2;

		if (input == 5 || input == inputHistory[0]) return;

		AddInputToHistory((byte)input);
	}

	void InputJump(bool pressed)
	{
		inputJump = Convert.ToInt16(pressed);
		
		if (_noInput == 0) return;

        if (pressed)
        {
			AddInputToHistory(10);

            if (_Grounded)
            {
                if (!_noJump)
                {
					ActionJump();
                }
            }
        }
        else
        {
            if (_jump == !_Grounded == true)
            {
                if (velocity.Y < 0)
                {
                    velocity.Y *= 0.6f;
                }
			}
        }
	}

	void InputAction1(bool pressed)
	{
		inputAction1 = Convert.ToInt16(pressed);

        if (_noInput == 0) return;

		playerClass.AttackLight(pressed);

		if (pressed)
		{
			AddInputToHistory(11);
		}
	}

	void InputAction2(bool pressed)
	{            
		if (_noInput == 0) return;

		if (pressed)
		{
			AddInputToHistory(12);
		}
	}

	public override void _UnhandledInput(InputEvent input)
	{
		if (input.IsEcho() || !input.IsActionType()) return;

		bool pressed = input.IsPressed();
		if (input.IsAction("playerRight"))
		{
			InputMove
			(
				pressed,
				Input.GetActionStrength("playerRight"),
				-Input.GetActionStrength("playerLeft")
			);

			input.Dispose();
			return;
		}
		if (input.IsAction("playerLeft"))
		{
			InputMove
			(
				pressed,
				-Input.GetActionStrength("playerLeft"),
				Input.GetActionStrength("playerRight")
			);

			input.Dispose();
			return;
		}

		if (input.IsAction("playerUp"))
		{
			InputLook
			(
				pressed,
				Input.GetActionStrength("playerUp"),
				-Input.GetActionStrength("playerDown")
			);

			input.Dispose();
			return;
		}
		if (input.IsAction("playerDown"))
		{
			InputLook
			(
				pressed,
				-Input.GetActionStrength("playerDown"),
				Input.GetActionStrength("playerUp")
			);

			input.Dispose();
			return;
		}

		//fuck ass why am i not able to switch case this :(
		if (input.IsAction("playerJump")){
			InputJump(pressed);
		} else 
		if (input.IsAction("playerAction1")){
			inputAction1 = Convert.ToInt16(pressed);
			InputAction1(pressed);
		} else
		if (input.IsAction("uiPause")){
			GetTree().Quit();
		}

		input.Dispose();
	}
}

public struct DrawRay{
	public Vector2 from;
	public Vector2 to;
	public Color color;
	public byte num;
}

public struct RaycastHit2D{
	public bool hit;
	public Vector2 pos;
	public Vector2 norm;
	public float distance;
}
