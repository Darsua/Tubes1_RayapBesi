using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MainBot : Bot
{
    /* A bot that drives forward and backward, and fires a bullet */
    bool movingForward;
    static void Main(string[] args)
    {
        new MainBot().Start();
    }

    MainBot() : base(BotInfo.FromFile("MainBot.json")) { }

    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.Gray;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;

        movingForward = true;

        while (IsRunning)
        {
            TurnRadarLeft(10);

        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        var distance = DistanceTo(e.X, e.Y);
        var enemy_energy = e.Energy;
        var enemy_speed = e.Speed;

        if(distance < 10){
            ReverseDirection();
        }

        var predictedX = e.X + Math.Cos(e.Direction) * enemy_speed;
        var predictedY = e.Y + Math.Sin(e.Direction) * enemy_speed;

        var bearing = BearingTo(predictedX, predictedY);

        var firepower = Math.Min(5, Energy);
        if(enemy_speed < 2) {
            firepower *= 2;
            TurnLeft(bearing);
            Fire(firepower);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }

    public void ReverseDirection()
    {
        if (movingForward)
        {
            Forward(-200);
            movingForward = false;
        }
        else
        {
            Forward(200);
            movingForward = true;
        }
    }

    public void runBro(){

    }


    /* Read the documentation for more events and methods */
}
