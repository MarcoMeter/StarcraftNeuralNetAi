using System.ComponentModel;
using System.Drawing;
using System.Linq;
using BroodWar;
using BroodWar.Api;
using BroodWar.Api.Enum;

namespace NetworkTraining
{
    public class TrainingModule : AiBase
    {
        #region Member
        private bool _showBullets;
        private bool _showVisibilityData;
        #endregion

        #region Events
        public override void OnStart()
        {
            Game.Write("The map is {0}, a {1} player map", Game.MapName, Game.StartLocations.Count);

            // Enable some cheat flags
            //Game.EnableFlag(Flag.UserInput);

            // Uncomment to enable complete map information
            //Game.EnableFlag(Flag.CompleteMapInformation);

            Game.Write("The match up is {0} v {1}", Game.Self.Race.Type, Game.Enemy.Race.Type);

            //send each worker to the mineral field that is closest to it
            foreach (var unit in Game.Self.Units)
            {
                if (unit.UnitType.IsWorker)
                {
                    var closestMineral = Game.Minerals.OrderBy(unit.Distance).FirstOrDefault();
                    if (closestMineral != null)
                        unit.RightClick(closestMineral, false);
                }
                else if (unit.UnitType.IsResourceDepot)
                {
                    //if this is a center, tell it to build the appropiate type of worker
                    if (unit.UnitType.Race.Type != RaceType.Zerg)
                    {
                        unit.Train(Game.Self.Race.Worker.Type);
                    }
                    else //if we are Zerg, we need to select a larva and morph it into a drone
                    {
                        var larva = unit.Larva.FirstOrDefault();
                        if (larva != null)
                            larva.Morph(BroodWar.Api.Enum.UnitType.Zerg_Drone);
                    }
                }
            }
        }

        public override void OnFrame()
        {
            if (_showVisibilityData)
                DrawVisibilityData();

            if (_showBullets)
                DrawBullets();

            var workerType = Game.Self.Race.Worker;
            var price = workerType.Price;
            var depot = Game.Self.Units.FirstOrDefault(u => u.UnitType.IsResourceDepot);
            //if our depot queue is empty and we have enouth supply and less than 16 workers
            if (depot != null
                && !depot.TrainingQueue.Any()
                && Game.Self.SupplyTotal - Game.Self.SupplyUsed >= price.Supply
                && Game.Self.Units.Count(u => u.UnitType == workerType) < 16)
            {
                if (depot.UnitType.Race.Type != RaceType.Zerg)
                {
                    depot.Train(workerType.Type);
                }
                else
                {
                    var larva = depot.Larva.FirstOrDefault();
                    if (larva != null)
                        larva.Morph(workerType.Type);
                }
            }

            DrawStats();
        }

        public override void OnUnitCreate(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been created at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnUnitMorph(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been morphed at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X,
            unit.Position.Y);
        }

        public override void OnUnitComplete(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been completed at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X,
                unit.Position.Y);
            if (!unit.UnitType.IsWorker || unit.Player.Id != Game.Self.Id)
                return;

            var closestMineral = Game.Minerals.OrderBy(unit.Distance).FirstOrDefault();
            if (closestMineral != null)
                unit.RightClick(closestMineral, false);
        }

        public override void OnUnitDiscover(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been discovered at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnUnitShow(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been spotted at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnUnitHide(Unit unit)
        {
            Game.SendText("A {0} [{1}] was last seen at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnUnitEvade(Unit unit)
        {
            Game.SendText("A {0} [{1}] was left accessible at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnUnitDestroy(Unit unit)
        {
            Game.SendText("A {0} [{1}] has been destroyed at ({2},{3})", unit.UnitType.Type, unit, unit.Position.X, unit.Position.Y);
        }

        public override void OnNukeDetect(Position target)
        {
            if (!target.IsUnknown)
                Game.Write("Nuclear Launch Detected at ({0},{1})", target.X, target.Y);
            else
                Game.Write("Nuclear Launch Detected");
        }

        public override void OnUnitRenegade(Unit unit)
        {
        }

        public override void OnSendText(string text)
        {
            switch (text)
            {
                case "/show bullets":
                    _showBullets = !_showBullets;
                    break;
                case "/show players":
                    ShowPlayers();
                    break;
                case "/show forces":
                    ShowForces();
                    break;
                case "/show visibility":
                    _showVisibilityData = !_showVisibilityData;
                    break;
                default:
                    Game.Write("You typed '{0}'!", text);
                    Game.SendText(text);
                    break;
            }
        }

        public override void OnReceiveText(Player player, string text)
        {
            Game.Write("{0} said '{1}'", player.Name, text);
        }

        public override void OnPlayerLeft(Player player)
        {
            Game.SendText("{0} left the game.", player.Name);
        }

        public override void OnSaveGame(string gameName)
        {
        }

        public override void OnEnd(bool isWinner)
        {
        }
        #endregion

        #region local functions
        private void DrawStats()
        {
            var myUnits = Game.Self.Units;
            Game.DrawTextScreen(5, 0, "I have {0} units:", myUnits.Count);
            var groups = myUnits.GroupBy(u => u.UnitType).ToDictionary(u => u.Key, u => u.Count());
            var line = 1;
            foreach (var group in groups)
            {
                Game.DrawTextScreen(5, 16 * line, "{0} {1}s", group.Value, group.Key.Type);
                line++;
            }
        }

        private void DrawBullets()
        {
            var bullets = Game.Bullets;
            foreach (var bullet in bullets)
            {
                var position = bullet.Position;
                var velocityX = bullet.VelocityX;
                var velocityY = bullet.VelocityY;
                if (bullet.Player == Game.Self)
                {
                    Game.DrawLineMap(position.X, position.Y, (int)(position.X + velocityX), (int)(position.Y + velocityY), Color.Green);
                    Game.DrawTextMap(position.X, position.Y, "{0}", bullet.Type);
                }
                else
                {
                    Game.DrawLineMap(position.X, position.Y, (int)(position.X + velocityX), (int)(position.Y + velocityY), Color.Red);
                    Game.DrawTextMap(position.X, position.Y, "{0}", bullet.Type);
                }
            }
        }

        private void DrawVisibilityData()
        {
            for (var x = 0; x < Game.MapWidth; x++)
            {
                for (var y = 0; y < Game.MapHeight; y++)
                {
                    if (Game.IsExplored(x, y))
                    {
                        Game.DrawDotMap(x * 32 + 16, y * 32 + 16, Game.IsVisible(x, y) ? Color.Green : Color.Blue);
                    }
                    else
                        Game.DrawDotMap(x * 32 + 16, y * 32 + 16, Color.Red);
                }
            }
        }

        private void ShowPlayers()
        {
            foreach (var player in Game.Players)
            {
                Game.Write("Player [{0}]: {1} is in force: {2}", player.Id, player.Name, player.Force.Name);
            }
        }

        private void ShowForces()
        {
            foreach (var force in Game.Forces)
            {
                Game.Write("Force {0} has the following players:", force.Name);
                foreach (var player in force.Players)
                {
                    Game.Write("  - Player [{0}]: {1}", player.Id, player.Name);
                }
            }
        }
        #endregion
    }
}