using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;
// ------------------------------------------------------------------
// RimRunnerz
// ------------------------------------------------------------------
// A tank bot that:
// 1. Orbits around the closest enemy (clockwise or counterclockwise)
// 2. Switches orbit direction when hitting something
// 3. Moves randomly when no enemies are detected
// 4. Uses precise, maximum power shooting when orbiting
// ------------------------------------------------------------------
public class RimRunnerz : Bot
{
    // Constants
    private const double ORBIT_DISTANCE = 150;  // Default distance to maintain from enemy
    private const double CLOSE_RANGE = 50;      // Range at which to fire immediately
    private const double MAX_RANGE = 250;       // Maximum range for targeting
    private const double DANGER_RANGE = 100;    // Range to consider enemies dangerous (for retreat)
    private const int DANGER_COUNT = 4;         // Number of close enemies to trigger retreat
    private const int TARGET_MEMORY = 3;        // Number of seconds to remember target
    private const double LOW_ENERGY = 25;       // Energy threshold for survival mode
    
    // Target tracking
    private bool hasTarget = false;
    private double targetX = 0;
    private double targetY = 0;
    private double targetVelocity = 0;
    private double targetHeading = 0;
    private long lastScanTime = 0;
    
    // Enemy tracking
    private Dictionary<string, EnemyInfo> enemies = new Dictionary<string, EnemyInfo>();
    private bool inRetreatMode = false;
    private bool inSurvivalMode = false;
    
    // Orbit direction (true = clockwise, false = counterclockwise)
    private bool orbitClockwise = true;
    
    // Random movement when no enemies
    private Random rand = new Random();
    private long lastDirectionChange = 0;
    
    // The main method starts our bot
    static void Main()
    {
        new RimRunnerz().Start();
    }
    
    // Constructor, which loads the bot config file
    RimRunnerz() : base(BotInfo.FromFile("RimRunnerz.json")) { }
    
    // Dipanggil ketika ronde baru dimulai
    public override void Run()
    {
        // Reset seluruh variabel status pada awal setiap ronde
        // Ini penting untuk mencegah bug setelah banyak putaran
        hasTarget = false;
        targetX = 0;
        targetY = 0;
        targetVelocity = 0;
        targetHeading = 0;
        lastScanTime = 0;
        enemies.Clear();
        inRetreatMode = false;
        inSurvivalMode = false;
        lastDirectionChange = 0;
        orbitClockwise = true;
        
        // Set warna
        BodyColor = Color.FromArgb(0x33, 0x33, 0x99);   // biru tua
        TurretColor = Color.FromArgb(0x66, 0x66, 0xCC); // biru sedang
        RadarColor = Color.FromArgb(0x99, 0x99, 0xFF);  // biru muda
        BulletColor = Color.FromArgb(0xFF, 0x33, 0x33); // merah
        ScanColor = Color.FromArgb(0xFF, 0xCC, 0x99);   // oranye muda
        
        // Set pergerakan independen untuk senjata dan radar
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        
        // Set parameter perputaran awal
        MaxSpeed = 5; // Batasi kecepatan untuk kontrol yang lebih baik
        
        // Main loop
        while (IsRunning)
        {
            // Bersihkan data lama dan periksa kondisi 
            if (TurnNumber % 10 == 0)
            {
                CleanupEnemyData();
                CheckForRetreat();
                
                // Periksa level energi untuk mode bertahan hidup
                inSurvivalMode = (Energy <= LOW_ENERGY);
            }
            
            // Tambahan reset setiap 500 putaran untuk menghindari masalah pergerakan yang aneh
            if (TurnNumber % 500 == 0)
            {
                // Reset beberapa parameter gerakan untuk mencegah perilaku aneh
                lastDirectionChange = TurnNumber;
                
                // Ganti arah orbit secara berkala
                orbitClockwise = !orbitClockwise;
            }
            
            if (inSurvivalMode)
            {
                SurvivalMode();
            }
            else if (inRetreatMode)
            {
                Retreat();
            }
            else if (!hasTarget || (TurnNumber - lastScanTime > TARGET_MEMORY * 10))
            {
                hasTarget = false;
                SpinMove();
            }
            else
            {
                SpinOrbitTarget();
            }
            
            if (TurnNumber % 20 < 10) {
                SetTurnRadarRight(45);
            } else {
                SetTurnRadarLeft(45);
            }
            
            Go();
        }
    }
    
    private void CleanupEnemyData()
    {
        List<string> toRemove = new List<string>();
        
        foreach (var entry in enemies)
        {
            if (TurnNumber - entry.Value.LastSeen > 10)
            {
                toRemove.Add(entry.Key);
            }
        }
        
        foreach (string key in toRemove)
        {
            enemies.Remove(key);
        }
        
        bool hasRecentEnemy = false;
        foreach (var entry in enemies)
        {
            if (TurnNumber - entry.Value.LastSeen <= 5)
            {
                hasRecentEnemy = true;
                break;
            }
        }
        
        if (!hasRecentEnemy)
        {
            hasTarget = false;
        }
    }
    
    private void CheckForRetreat()
    {
        int closeEnemies = 0;
        
        foreach (var entry in enemies)
        {
            double distance = DistanceTo(entry.Value.X, entry.Value.Y);
            if (distance <= DANGER_RANGE)
            {
                closeEnemies++;
            }
        }
        
        inRetreatMode = (closeEnemies >= DANGER_COUNT);
    }
    
    private void SpinMove()
    {
        // Change direction periodically or when movement is complete
        if (TurnNumber - lastDirectionChange > 20 || DistanceRemaining < 5)
        {
            // Turn a random amount
            double turnAmount = rand.NextDouble() * 180 - 90; // -90 to +90 degrees
            SetTurnRight(turnAmount);
            
            // Move forward a random amount to avoid staying in one place
            double moveAmount = rand.NextDouble() * 150 + 100; // 100 to 250 units
            SetForward(moveAmount);
            
            lastDirectionChange = TurnNumber;
        }
        
        SetTurnRadarLeft(45);
    }
    private void SurvivalMode()
    {
        // Find the safest direction to move (away from ALL enemies)
        double safeX = X;
        double safeY = Y;
        bool hasEnemies = false;
        
        foreach (var entry in enemies)
        {
            // Only consider recent enemy sightings
            if (TurnNumber - entry.Value.LastSeen <= 10)
            {
                hasEnemies = true;
                
                // Calculate vector away from each enemy
                double dx = X - entry.Value.X;
                double dy = Y - entry.Value.Y;
                
                // Normalize and add to escape vector
                double dist = Math.Sqrt(dx*dx + dy*dy);
                if (dist > 0)
                {
                    double weight = 200 / Math.Max(dist, 20); // Higher weight for closer enemies
                    safeX += (dx / dist) * weight;
                    safeY += (dy / dist) * weight;
                }
            }
        }
        
        if (hasEnemies)
        {
            // Turn toward safe point
            double escapeAngle = DirectionTo(safeX, safeY);
            double turnAngle = NormalizeAngle(escapeAngle - Direction);
            SetTurnLeft(turnAngle);
            
            // Only move if facing approximately the right direction to conserve energy
            if (Math.Abs(turnAngle) < 45)
            {
                SetForward(100);
            }
            else
            {
                // If not facing right direction, stop to reorient
                SetForward(0);
            }
        }
        else
        {
            // No enemies? Move to center of battlefield for better position
            double centerX = ArenaWidth / 2;
            double centerY = ArenaHeight / 2;
            
            // If already near center, move randomly
            if (DistanceTo(centerX, centerY) < 100)
            {
                SpinMove();
            }
            else
            {
                double centerAngle = DirectionTo(centerX, centerY);
                double turnAngle = NormalizeAngle(centerAngle - Direction);
                SetTurnLeft(turnAngle);
                SetForward(50);
            }
        }
        
        // Focus on scanning, don't waste energy shooting
        if (TurnNumber % 20 < 10) {
            SetTurnRadarRight(45);
        } else {
            SetTurnRadarLeft(45);
        }
    }
    
    // Spin orbit around the current target (like a moon around a planet)
    private void SpinOrbitTarget()
    {
        // Calculate current distance to target
        double distance = DistanceTo(targetX, targetY);
        
        // If target is too far, clear target
        if (distance > MAX_RANGE)
        {
            hasTarget = false;
            return;
        }
        
        // Calculate bearing to target for precise shooting
        double absoluteBearing = DirectionTo(targetX, targetY);
        double gunBearing = GunBearingTo(targetX, targetY);
        
        // Implement predictive aiming - lead the target based on its velocity and heading
        double bulletPower = 2.0;
        double bulletSpeed = 20 - 3 * bulletPower;
        double timeToHit = distance / bulletSpeed;
        
        // Predict future position if we have velocity and heading data
        double futureX = targetX;
        double futureY = targetY;
        
        if (Math.Abs(targetVelocity) > 0.1)
        {
            // Calculate future position
            double targetMovementAngle = targetHeading;
            // Convert degrees to radians before using sin/cos
            double angleInRadians = ToRadians(targetMovementAngle);
            
            futureX = targetX + Math.Sin(angleInRadians) * targetVelocity * timeToHit;
            futureY = targetY + Math.Cos(angleInRadians) * targetVelocity * timeToHit;
            
            // Make sure the predicted position is within battlefield boundaries
            if (IsPointInsideBattlefield(futureX, futureY)) {
                // Adjust gun to predicted position
                gunBearing = GunBearingTo(futureX, futureY);
            } else {
                // If prediction is outside battlefield, aim at current position
                futureX = targetX;
                futureY = targetY;
                gunBearing = GunBearingTo(targetX, targetY);
            }
        }
        
        // Turn gun toward predicted position for precise shooting
        SetTurnGunLeft(gunBearing);
        
        // Calculate orbit angle (perpendicular to target bearing to orbit like a moon)
        double orbitAngle;
        if (orbitClockwise)
        {
            orbitAngle = absoluteBearing + 90; // Clockwise orbit
        }
        else
        {
            orbitAngle = absoluteBearing - 90; // Counterclockwise orbit
        }
        
        // Turn toward orbit angle to maintain circular movement
        double turnAngle = NormalizeAngle(orbitAngle - Direction);
        SetTurnLeft(turnAngle);
        
        // Adjust speed and direction based on distance to target
        if (distance > ORBIT_DISTANCE + 20)
        {
            // Too far, move closer
            MaxSpeed = 5;
            SetForward(distance - ORBIT_DISTANCE);
        }
        else if (distance < ORBIT_DISTANCE - 20)
        {
            // Too close, move away
            MaxSpeed = 5;
            SetBack(ORBIT_DISTANCE - distance);
        }
        else
        {
            // Good distance, maintain orbit velocity
            MaxSpeed = 5;
            SetForward(50);
        }
        
        // Selalu tembak jika target terlihat dalam 3 detik terakhir (30 putaran)
        // Pastikan energi cukup untuk selalu siap bertahan
        if (TurnNumber - lastScanTime <= TARGET_MEMORY * 10)
        {
            // Tembak hanya jika senapan terarah dengan tepat dan kita memiliki target yang valid
            if (Math.Abs(gunBearing) <= 3 && GunHeat == 0 && hasTarget)
            {
                // Verifikasi tidak menembak ke dinding
                // Periksa apakah posisi prediksi ada dalam batas medan perang
                bool validTarget = IsPointInsideBattlefield(futureX, futureY);
                
                if (validTarget)
                {
                    double firePower = 2.0; // Gunakan daya sedang sebagai default untuk keseimbangan
                    
                    // Sesuaikan kekuatan tembakan berdasarkan energi tersisa
                    if (Energy > 50)
                    {
                        // Energi cukup banyak, bisa agresif sedikit
                        if (distance < CLOSE_RANGE)
                        {
                            firePower = 3.0; // Jarak dekat = kekuatan maksimal
                        }
                        else if (distance < MAX_RANGE)
                        {
                            firePower = 2.0; // Jarak sedang = kekuatan seimbang
                        }
                        else
                        {
                            firePower = 1.0; // Jarak jauh = kekuatan rendah untuk peluru cepat
                        }
                    }
                    else if (Energy > LOW_ENERGY)
                    {
                        // Energi terbatas, waspada dan efisien
                        if (distance < CLOSE_RANGE)
                        {
                            firePower = 2.0; // Kurangi kekuatan pada jarak dekat untuk menghemat energi
                        }
                        else
                        {
                            firePower = 1.0; // Gunakan kekuatan minimal untuk jarak lainnya
                        }
                    }
                    else
                    {
                        // Energi sangat rendah, prioritaskan kelangsungan hidup
                        // Masih menembak tapi dengan kekuatan minimal
                        firePower = 0.5; // Tembakan pembela diri terlemah
                    }
                    
                    Fire(firePower);
                }
            }
        }
    }
    
    
    // Retreat from multiple enemies
    private void Retreat()
    {
        // Find direction away from concentration of enemies
        double escapeX = X;
        double escapeY = Y;
        
        foreach (var entry in enemies)
        {
            // Calculate vector away from each enemy
            double dx = X - entry.Value.X;
            double dy = Y - entry.Value.Y;
            
            // Normalize and add to escape vector
            double dist = Math.Sqrt(dx*dx + dy*dy);
            if (dist > 0)
            {
                escapeX += (dx / dist) * (DANGER_RANGE / dist);
                escapeY += (dy / dist) * (DANGER_RANGE / dist);
            }
        }
        
        // Turn toward escape point
        double escapeAngle = DirectionTo(escapeX, escapeY);
        double turnAngle = NormalizeAngle(escapeAngle - Direction);
        SetTurnLeft(turnAngle);
        
        // Move away quickly
        SetForward(100);
        
        // Continue scanning while retreating
        SetTurnRadarLeft(45);
    }
    
    // Normalize angle to range -180 to +180
    private double NormalizeAngle(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
    
    // Helper method to check if a point is inside the battlefield
    private bool IsPointInsideBattlefield(double x, double y)
    {
        // Add a small margin to avoid shooting right at the edge
        double margin = 20;
        return x > margin && x < ArenaWidth - margin && 
               y > margin && y < ArenaHeight - margin;
    }
    
    // Convert degrees to radians
    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
    
    // Generate a unique ID for an enemy based on its coordinates
    private string GenerateEnemyId(double x, double y)
    {
        return x.ToString("0.0") + ":" + y.ToString("0.0");
    }
    
    // Find the closest enemy
    private EnemyInfo FindClosestEnemy()
    {
        EnemyInfo closest = null;
        double closestDistance = double.MaxValue;
        
        foreach (var entry in enemies)
        {
            double distance = DistanceTo(entry.Value.X, entry.Value.Y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = entry.Value;
            }
        }
        
        return closest;
    }
    
    // We scanned another bot
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Update scan time
        lastScanTime = TurnNumber;
        
        // Get enemy coordinates
        double enemyX = e.X;
        double enemyY = e.Y;
        double distance = DistanceTo(enemyX, enemyY);
        
        // Generate a unique ID for this enemy
        string enemyId = GenerateEnemyId(enemyX, enemyY);
        
        // Update enemy information
        if (enemies.ContainsKey(enemyId))
        {
            enemies[enemyId].X = enemyX;
            enemies[enemyId].Y = enemyY;
            enemies[enemyId].LastSeen = TurnNumber;
            enemies[enemyId].Velocity = e.Speed;
            enemies[enemyId].Direction = e.Direction;
        }
        else
        {
            enemies[enemyId] = new EnemyInfo(enemyX, enemyY, TurnNumber, e.Speed, e.Direction);
        }
        
        // Check if we should update our target (if this is the closest enemy)
        EnemyInfo closestEnemy = FindClosestEnemy();
        if (closestEnemy != null)
        {
            // Make sure it's a recent sighting (within last 5 turns)
            if (TurnNumber - closestEnemy.LastSeen <= 5)
            {
                targetX = closestEnemy.X;
                targetY = closestEnemy.Y;
                targetVelocity = closestEnemy.Velocity;
                targetHeading = closestEnemy.Direction;
                hasTarget = true;
            }
        }
        
        // Check if we should retreat
        CheckForRetreat();
        
        // If this is a very close enemy, shoot immediately but check battlefield boundaries
        if (distance < CLOSE_RANGE && GunHeat == 0 && IsPointInsideBattlefield(enemyX, enemyY))
        {
            double gunBearing = GunBearingTo(enemyX, enemyY);
            if (Math.Abs(gunBearing) <= 5)
            {
                // For very close enemies, shoot immediately with power 3.0
                Fire(3.0);
            }
        }
    }
    
    // We hit another bot - switch orbit direction
    public override void OnHitBot(HitBotEvent e)
    {
        // Fire at power 3.0 for maximum damage
        Fire(3.0);
        
        // Back up
        Back(50);
        
        // Switch orbit direction
        orbitClockwise = !orbitClockwise;
    }
    
    // We hit a wall - switch orbit direction
    public override void OnHitWall(HitWallEvent e)
    {
        // Back up from the wall
        Back(30);
        
        // Turn away from wall
        TurnRight(90);
        
        // Switch orbit direction
        orbitClockwise = !orbitClockwise;
    }
    
    // Class to track enemy information
    private class EnemyInfo
    {
        public double X { get; set; }
        public double Y { get; set; }
        public long LastSeen { get; set; }
        public double Velocity { get; set; }
        public double Direction { get; set; }
        
        public EnemyInfo(double x, double y, long lastSeen)
        {
            X = x;
            Y = y;
            LastSeen = lastSeen;
            Velocity = 0;
            Direction = 0;
        }
        
        public EnemyInfo(double x, double y, long lastSeen, double velocity, double direction)
        {
            X = x;
            Y = y;
            LastSeen = lastSeen;
            Velocity = velocity;
            Direction = direction;
        }
    }
}