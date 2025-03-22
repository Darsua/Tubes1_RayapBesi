using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class IkanLele : Bot
{
    static void Main(string[] args)
    {
        new IkanLele().Start();
    }

    IkanLele() : base(BotInfo.FromFile("IkanLele.json")) {
        GunTurnRate = 30;
        RadarTurnRate = 30;
        TurnRate = 30;
     }

    public override void Run()
    {
        BodyColor = Color.Gray;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;

        do
        {
            SetTurnLeft(10_000);
            MaxSpeed = 5;
            Forward(10_000);
            Rescan();
        } while (IsRunning);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Kalkulasi Radar
        double radarTurn = NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y));
        double enemyDistance = DistanceTo(e.X, e.Y);

        double extraTurn = Math.Min(Math.Atan(36.0 / enemyDistance) * (180 / Math.PI), MaxRadarTurnRate);

        // Kalkulasi Senjata
        // double bulletSpeed = CalcBulletSpeed(2);
        // double timeImpact = enemyDistance / bulletSpeed;
        // double futureX = e.X + Math.Cos(e.Direction) * e.Speed * timeImpact * 0.5;
        // double futureY = e.Y + Math.Sin(e.Direction) * e.Speed * timeImpact * 0.5;
        double gunTurn = NormalizeRelativeAngle(GunBearingTo(e.X, e.Y));

        radarTurn += radarTurn > 0 ? extraTurn : -extraTurn;

        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);
        if(Math.Abs(GunBearingTo(e.X,e.Y)) < 10 && Energy > 20){
            Fire(2);
        } 
        
    }

    public override void OnHitBot(HitBotEvent e)
    {
        var bearing = BearingTo(e.X, e.Y);
        if (bearing > -10 && bearing < 10)
        {
            Fire(3);
        }
        if (e.IsRammed)
        {
            TurnLeft(10);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Forward(-200);
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        Forward(50);
        TurnRight(45);
    }
}
