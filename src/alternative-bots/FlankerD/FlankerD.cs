using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

namespace FlankerD;

public class FlankerD : Bot
{

    static void Main()
    {
        new FlankerD().Start();
    }

    FlankerD() : base(BotInfo.FromFile("FlankerD.json")) { }

    public override void Run()
    {
        BodyColor = Color.Purple;
        // By Darsu
        while (IsRunning)
        {
            TurnLeft(BearingTo(ArenaWidth * 0.5, ArenaHeight * 0.5));
            TurnRadarLeft(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // How far do we want to keep our distance?
        var flankDistance = 100;

        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y) * 1.5);

        var targetDirection = (e.Speed < 0 ? (e.Direction + 180) % 360 : e.Direction) * Math.PI / 180;
        var xFlank = e.X - Math.Cos(targetDirection) * flankDistance;
        var yFlank = e.Y - Math.Sin(targetDirection) * flankDistance;
        var flankBearing = BearingTo(xFlank, yFlank);

        SetTurnLeft(flankBearing);
        SetForward(Math.Min(DistanceTo(xFlank, yFlank), DistanceTo(e.X, e.Y) - 70));

        // If the enemy is facing us, we need to get around them.
        if (RadarDirection - e.Direction <= 90 || RadarDirection - e.Direction >= 270)
        {
            if (DistanceTo(e.X, e.Y) <= 1 * flankDistance)
            {
                SetTurnLeft(CalcBearing(RadarDirection + 120)); // Get away from the enemy if too close
            }
            else if (DistanceTo(e.X, e.Y) <= 2 * flankDistance) // Start circling the enemy to get around
            {
                SetTurnLeft(CalcBearing(RadarDirection + 90));
            }
            else // Approach with a diagonal
            {
                SetTurnLeft(CalcBearing(RadarDirection + 45));
            }
        }

        // Avoid walls
        if (X < 150 || X > ArenaWidth - 150 || Y < 150 || Y > ArenaHeight - 150)
        {
            SetTurnLeft(BearingTo(ArenaWidth * 0.5, ArenaHeight * 0.5));
        }

        var firepower = 3 * flankDistance / DistanceTo(e.X, e.Y);
        LeadFire(e, firepower);
    }

    private void LeadFire(ScannedBotEvent e, double firepower)
    {
        // Lead your shots!
        var targetDistance = DistanceTo(e.X, e.Y);
        var targetSpeed = Math.Abs(e.Speed);
        var targetDirection = (e.Speed < 0 ? (e.Direction + 180) % 360 : e.Direction) * Math.PI / 180; // Is the target in reverse?

        var bulletSpeed = CalcBulletSpeed(firepower);
        var deltaTime = targetDistance / bulletSpeed;

        var xLead = e.X + Math.Cos(targetDirection) * targetSpeed * deltaTime;
        var yLead = e.Y + Math.Sin(targetDirection) * targetSpeed * deltaTime;

        var i = Math.Floor(targetDistance / 100);
        for (; i > 0; i--)
        {
            targetDistance = DistanceTo(xLead, yLead);
            deltaTime = targetDistance / bulletSpeed;
            xLead = e.X + Math.Cos(targetDirection) * targetSpeed * deltaTime;
            yLead = e.Y + Math.Sin(targetDirection) * targetSpeed * deltaTime;
        }

        SetTurnGunLeft(GunBearingTo(xLead, yLead));

        // Only shoot if close to target lead.
        if (Math.Abs(GunBearingTo(xLead, yLead)) < 5)
        {
            Fire(firepower);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnRadarLeft(RadarBearingTo(e.X, e.Y) * 1.2);
    }
}
