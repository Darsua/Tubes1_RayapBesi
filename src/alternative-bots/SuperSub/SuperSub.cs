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
    int dmg;

    bool stop;
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
        stop = false;
        dmg = 3;

        while (IsRunning)
        {   
            if(!stop){
                MaxSpeed = 5;
                SetForward(40_000);
                SetTurnLeft(40_000);
            } else {
                Stop();
            }
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        double enemyDistance = DistanceTo(e.X, e.Y);
        if(EnemyCount <= 2 && Energy > 30){
            SetFireAssist(true);
            SetTurnLeft(bearing * 10_000);
            if(enemyDistance < 150){
                stop = true;
            } else {
                SetForward(dist);
                stop = false;
            }
        } 

        if(EnemyCount == 1){
            SetFireAssist(false);
            SetTurnLeft(bearing * 10_000);
            if(enemyDistance < 150){
                stop = true;
            } else {
                SetForward(dist);
                stop = false;
            }
        }
        
        if(Energy < 60){
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
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if(isLeft){
            TurnRight(NormalizeRelativeAngle(90 - (Direction - e.Bullet.Direction)));
            isLeft = false;
        } else {
            TurnLeft(NormalizeRelativeAngle(90 - (Direction - e.Bullet.Direction)));
            isLeft = true;
        }   
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