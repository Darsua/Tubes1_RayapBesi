using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class IkanLele : Bot
{
    bool isLeft;
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
        isLeft = true;

        do
        {
            SetTurnRadarRight(double.PositiveInfinity);
            SetTurnLeft(10_000);
            MaxSpeed = 5;
            SetForward(10_000);
            Go();
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
        double bulletSpeed = CalcBulletSpeed(2);
        double timeImpact = enemyDistance / bulletSpeed;
        double futureX = e.X + Math.Cos(e.Direction) * e.Speed * timeImpact * 0.5;
        double futureY = e.Y + Math.Sin(e.Direction) * e.Speed * timeImpact * 0.5;
        double gunTurn = NormalizeRelativeAngle(GunBearingTo(futureX, futureY));

        radarTurn += radarTurn > 0 ? extraTurn : -extraTurn;

        SetTurnRadarLeft(radarTurn);
        SetTurnGunLeft(gunTurn);
        if(Math.Abs(GunBearingTo(e.X,e.Y)) < 10){
            SetFireAssist(true);
            Fire(3);
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
            SetTurnRadarLeft(NormalizeRelativeAngle(RadarBearingTo(e.X, e.Y)));
            SetTurnGunLeft(NormalizeRelativeAngle(GunBearingTo(e.X, e.Y)));
            SetTurnLeft(10);
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
