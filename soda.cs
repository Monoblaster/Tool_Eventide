datablock AudioProfile(SodaOpenSound)
{
	filename    = "./assets/canopen.wav";
	description = AudioClose3d;
	preload = true;
};
datablock AudioProfile(SodaDrink1Sound : SodaOpenSound)
{
	filename    = "./assets/gulp1.wav";
};
datablock AudioProfile(SodaDrink2Sound : SodaOpenSound)
{
	filename    = "./assets/gulp2.wav";
};
datablock AudioProfile(SodaDrink3Sound : SodaOpenSound)
{
	filename    = "./assets/gulp3.wav";
};

datablock ItemData(SodaItem)
{
	className = "Weapon";

	shapeFile = "./assets/sodacan.dts";
	emap = 1;

	mass = 1;
	density = 0.2;
	elasticity = 0.2;
	friction = 0.6;
	canDrop = 1;
	
	uiName = "Soda";
	iconName = "./assets/sodacan";

	doColorShift = false;
	
	image = SodaImage;
};

$c = -1;
datablock shapeBaseImageData(SodaImage)
{
	className = "WeaponImage";

	shapeFile = "./assets/sodacan.dts";
	emap = true;

	mountPoint = 0;
	offset = "-0.01 0.1 0";

	armReady = true;
	doColorShift = FlareGunItem.doColorShift;
	colorShiftColor = FlareGunItem.colorShiftColor;

	item = FlareGunItem;

	stateName[$c++] = "ready";
	stateTransitionOnTriggerDown[$c] = "fire";

	stateName[$c++] = "fire";
	stateFire[$c] = true;
	stateTimeOutValue[$c] = 0.01;
	stateTransitionOnTriggerUp[$c] = "open";

	stateName[$c++] = "open";
	stateScript[$c] = "onOpen";
	stateTimeOutValue[$c] = 1;
	stateTransitionOnTimeout[$c] = "drink";

	statename[$c++] = "drink";
	stateScript[$c] = "onDrink";
	stateTimeOutValue[$c] = 999999;
};

function sodaImage::OnOpen(%data,%obj,%slot)
{
	serverPlay3D("SodaOpenSound",%obj.getPosition());
	%obj.playThread(2,"shiftleft");
}

function SpeedEffect::Spawn(%e,%obj)
{
	%obj.AddMoveSpeedModifier(%e.mod);
}

function SpeedEffect::Despawn(%e,%obj)
{
	%obj.AddMoveSpeedModifier(-%e.mod);
}

function sodaImage::OnDrink(%data,%obj,%slot)
{
	serverPlay3D("SodaDrink" @ getRandom(1,3) @ "Sound",%obj.getPosition());
	%obj.playThread(2,"jump");
	if(%e = %obj.StatusEffect_FindName("SodaBoost"))
	{
		%e.duration(%e.getDuration() + 6000);
	}
	else
	{
		%obj.StatusEffect("SpeedEffect","SodaBoost","mod=0.25;").duration(6000);
	}
	%obj.unmountImage(%slot);
	%c = %obj.client;
	if(isObject(%c))
	{
		messageClient(%c, 'MsgItemPickup', '', %obj.currtool, 0,true);
	}
	%obj.tool[%obj.currTool] = "";
	new projectile()
	{
		dataBlock = "SodaProjectile";
		initialPosition = %obj.getMuzzlePoint(%slot);
		initialVelocity = 0;
	}.explode();
}
	
datablock DebrisData(SodaDebris)
{
	elasticity = 0.5;
	gravModifier = 0.9;
	lifeTime = 5;
	maxSpinSpeed = 1000;
	numBounces = 2;
	fade = true;
	snapOnMaxBounce = false;
	staticOnMaxBounce = true;
	shapeFile = "./assets/sodacan.dts";
	velocity = 0;
};

datablock ExplosionData(SodaExplosion)
{
	debris = "SodaDebris";
	debrisNum = 1;
	debrisPhiMax = 360;
	debrisPhiMin = 180;
	debrisThetaMax = 360;
	debrisThetaMin = 0;
	debrisVelocity = 2;
};

datablock ProjectileData(SodaProjectile)
{
	explosion = "SodaExplosion";
};