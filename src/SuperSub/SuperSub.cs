using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class SuperSub : Bot
{
    /* A bot that drives forward and backward, and fires a bullet */
    bool movingForward;
    bool isLeft;
    int dist = 40_000;

    int dmg = 3;
    static void Main(string[] args)
    {
        new SuperSub().Start();
    }

    SuperSub() : base(BotInfo.FromFile("SuperSub.json")) { }

    public override void Run()
    {
        BodyColor = Color.Gray;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;

        movingForward = true;
        isLeft = true;

        while (IsRunning)
        {   
            MaxSpeed = 5;
            SetForward(40_000);
            SetTurnLeft(40_000);
            WaitFor(new TurnCompleteCondition(this));
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        double enemyDistance = DistanceTo(e.X, e.Y);
        if(EnemyCount <= 2){
            SetTurnLeft(bearing * 10_000);
            if(enemyDistance < 200){
                Stop();
            } else {
                SetForward(dist);
            }
        } 
        
        if(Energy < 20){
            dmg = 2;
        }
        Fire(dmg);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -10 && bearing < 10)
        {
            Fire(dmg);
        }
        if (e.IsRammed)
        {
            Fire(dmg);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public void ReverseDirection()
    {
        SetTurnLeft(0);
        if (movingForward)
        {
            SetBack(1000);
            movingForward = false;
        }
        else
        {
            SetForward(1000);
            movingForward = true;
        }

        SetForward(40_000);
        SetTurnLeft(40_000);
        WaitFor(new TurnCompleteCondition(this));
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if(isLeft){
            SetTurnRight(NormalizeRelativeAngle(90 - (Direction - e.Bullet.Direction))*1_000);
            isLeft = false;
        } else {
            SetTurnLeft(NormalizeRelativeAngle(90 - (Direction - e.Bullet.Direction))*1_000);
            isLeft = true;
        }   
        SetForward(40_000);

        Rescan();
    }
}

internal class TurnCompleteCondition : Condition
{
    private SuperSub SuperSub;

    public TurnCompleteCondition(SuperSub SuperSub)
    {
        this.SuperSub = SuperSub;
    }
}