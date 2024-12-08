﻿Console.WriteLine("Do you want to play in small, medium or large game?");
string input = Console.ReadLine()!;
while (input != "small" && input != "medium" && input != "large")
    input = Console.ReadLine()!;
int gameSize;
gameSize = input switch
{
    "small" => 2,
    "medium" => 3,
    "large" => 4,
    _ => 2
};
Console.Clear();

new FountainOfObjectsGame(gameSize).Run();
public class FountainOfObjectsGame
{
    Player player = new Player();
    Map map;
    Fountain fountain = new Fountain();
    public FountainOfObjectsGame(int gameSize) { map = new Map(gameSize); }
    public void Run()
    {
        DisplayIntro();
        while (!(fountain.IsActivated && player.Row == 0 && player.Column == 0))
        {
            Console.WriteLine(player.ToString());
            Surroundings.Print(map, player, fountain);
            Console.WriteLine("What do you want to do?");

            Console.ForegroundColor = ConsoleColor.Cyan;
            string? input = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;

            CommandExecution(input!);
            Console.WriteLine("--------------------------------------------------------------------------------------------");

            Maelstrom.Event(player, map);
            if (Pit.Event(player, map))
                break;
            if (Amarock.Event(player, map))
                break;
        }
        if (fountain.IsActivated && player.Row == 0 && player.Column == 0)
            PlayerWon();
    }
    private void CommandExecution(string input)
    {
        ICommand? moveCommand = input switch
        {
            "move north" => new MoveNorth(),
            "move south" => new MoveSouth(),
            "move west" => new MoveWest(),
            "move east" => new MoveEast(),
            "shoot north" => new ShootNorth(),
            "shoot south" => new ShootSouth(),
            "shoot west" => new ShootWest(),
            "shoot east" => new ShootEast(),
            _ => null
        };
        if (moveCommand != null)
            moveCommand.Run(player, map);
        else if (input == "enable fountain")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (fountain.IsActivated)
                Console.WriteLine("You already activated the fountain!");
            else if (!fountain.TryActivating(map, player))
                Console.WriteLine("You are not in the room where the fountain is!");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (input == "help")
            DisplayHelp();
        else
            Console.WriteLine("There is no such command.");
    }
    private static void DisplayIntro()
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("You enter the Cavern of Objects, a maze of rooms filled with dangerous pits in search of the Fountain of Objects.");
        Console.WriteLine("Light is visible only in the entrance, and no other light is seen anywhere in the caverns.");
        Console.WriteLine("You must navigate the Caverns with your other senses.");
        Console.WriteLine("Find the Fountain of Objects, activate it, and return to the entrance.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine("Look out for pits. You will feel a breeze if a pit is in an adjacent room.\nIf you enter a room with a pit, you will die.\n");
        Console.WriteLine("Maelstroms are violent forces of sentient wind.\n" +
                          "Entering a room with one could transport you to any other location in the caverns.\n" +
                          "You will be able to hear their growling and groaning in nearby rooms.\n");
        Console.WriteLine("Amaroks roam the caverns. Encountering one is certain death, but you can smell their rotten stench in nearby rooms.\n");
        Console.WriteLine("You carry with you a bow and a quiver of arrows.\nYou can use them to shoot monsters in the caverns but be warned: you have a limited supply.\n");
        Console.WriteLine("You can type in \"help\" to get a list of commands. Good luck!");
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine();
    }
    private static void DisplayHelp()
    {
        Console.WriteLine("\nmove north, move south, move west, move east - moves the player");
        Console.WriteLine("shoot north, shoot south, shoot west, shoot east - shoots an arrow");
        Console.WriteLine("enable fountain - attempts to activate fountain");
    }
    private void PlayerWon()
    {
        Surroundings.Print(map, player, fountain);
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("The Fountain of Objects has been reactivated, and you have escaped with your life!");
        Console.WriteLine("You Win!");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
public class Player
{
    public int Row { get; private set; }
    public int Column { get; private set; }
    public int Arrows { get; private set; } = 5;
    public override string ToString() { return $"You are in the room at (Row={Row}, Column={Column}) and have {Arrows} arrows."; }
    public void ChangeCoordinates(int x, int y) { Row = x; Column = y; }
    public void ArrowDecrement() => Arrows--;
}
public class Map // knows rooms/grid; knows how to check did player enter specific room; or what room is in specific coordinates; and validates move commands
{
    public RoomType[,] Room { get; private set; }
    public Map(int gameSize) // constructor that initializes rooms - entrance, fountain, pits, maelstroms, amarocks
    {
        Room = new RoomType[2 * gameSize, 2 * gameSize];
        Room[0, gameSize] = RoomType.FountainOfObjects;
        Room[0, 0] = RoomType.Entrance;

        for (int i = 2; i < 2 * gameSize; i += gameSize)
            for (int j = 2; j < 2 * gameSize; j += gameSize)
                Room[i, j] = RoomType.Pit;

        if (gameSize < 4)
            Room[2 * gameSize - 1, 1] = RoomType.Maelstrom;
        else
        {
            Room[2 * gameSize - 1, 1] = RoomType.Maelstrom;
            Room[2 * gameSize - 5, gameSize] = RoomType.Maelstrom;
        }

        if (gameSize == 2)
            Room[gameSize, 0] = RoomType.Amarock;
        else if (gameSize == 3)
        {
            Room[gameSize, 0] = RoomType.Amarock;
            Room[gameSize, gameSize] = RoomType.Amarock;
        }
        else
        {
            Room[gameSize, 0] = RoomType.Amarock;
            Room[gameSize, gameSize] = RoomType.Amarock;
            Room[0, 2 * gameSize - 1] = RoomType.Amarock;
        }
    }
    public bool MoveValidation(int row, int column)
    {
        if (row < Room.GetLength(0) && column < Room.GetLength(1) && row >= 0 && column >= 0)
            return true;
        Console.ForegroundColor = ConsoleColor.Gray;
        if (row == -1 && column == 0)
            Console.WriteLine("You are not done yet!");
        else
            Console.WriteLine("You bumped into a wall...");
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }
    public bool RoomIs(RoomType room, int row, int column) => Room[row, column] == room;
    public void ClearRoom(int row, int column) => Room[row, column] = RoomType.Empty;
}
public static class Surroundings // describes to player what are the surroundings, knows how to check adjecent rooms for danger
{
    public static void Print(Map map, Player player, Fountain fountain)
    {
        if (map.RoomIs(RoomType.FountainOfObjects, player.Row, player.Column))
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            if (fountain.IsActivated)
                Console.WriteLine("You hear the rushing waters from the Fountain of Objects. It has been reactivated!");
            else
                Console.WriteLine("You hear water dripping in this room. The Fountain of Objects is here!");
            Console.ForegroundColor = ConsoleColor.White;
        }
        if (AdjacentRoomsCheck(RoomType.Pit, map, player.Row, player.Column))
            Console.WriteLine("You feel a draft. There is a pit in a nearby room.");
        if (AdjacentRoomsCheck(RoomType.Amarock, map, player.Row, player.Column))
            Console.WriteLine("You can smell the rotten stench of an amarok in a nearby room.");
        if (AdjacentRoomsCheck(RoomType.Maelstrom, map, player.Row, player.Column))
            Console.WriteLine("You hear the growling and groaning of a maelstrom nearby.");
        if (player.Row == 0 && player.Column == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("You see light coming from the cavern entrance.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    private static bool AdjacentRoomsCheck(RoomType room, Map map, int row, int column)
    {
        if (row < map.Room.GetLength(0) - 1 && map.Room[row + 1, column] == room) return true;
        if (row > 0 && map.Room[row - 1, column] == room) return true;

        if (column < map.Room.GetLength(1) - 1 && map.Room[row, column + 1] == room) return true;
        if (column > 0 && map.Room[row, column - 1] == room) return true;

        if (row > 0 && column > 0 && map.Room[row - 1, column - 1] == room) return true;
        if (row < map.Room.GetLength(0) - 1 && column < map.Room.GetLength(1) - 1 && map.Room[row + 1, column + 1] == room) return true;

        if (column < map.Room.GetLength(1) - 1 && row > 0 && map.Room[row - 1, column + 1] == room) return true;
        if (column > 0 && row < map.Room.GetLength(0) - 1 && map.Room[row + 1, column - 1] == room) return true;
        return false;
    }
}
public static class Maelstrom
{
    public static void Event(Player player, Map map)
    {
        if (map.Room[player.Row, player.Column] == RoomType.Maelstrom)
        {
            map.Room[Math.Clamp(player.Row + 1, 0, map.Room.GetLength(0) - 1), Math.Clamp(player.Column - 2, 0, map.Room.GetLength(1) - 1)] = RoomType.Maelstrom;
            map.Room[player.Row, player.Column] = RoomType.Empty;
            player.ChangeCoordinates(Math.Clamp(player.Row - 1, 0, map.Room.GetLength(0) - 1), Math.Clamp(player.Column + 2, 0, map.Room.GetLength(1) - 1));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("You were blown away by a Maelstrom.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
public static class Pit
{
    public static bool Event(Player player, Map map)
    {
        if (map.RoomIs(RoomType.Pit, player.Row, player.Column))
        {
            Console.WriteLine(player.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("You felt into a pit.");
            Console.WriteLine("You lost...");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        return false;
    }
}
public static class Amarock
{
    public static bool Event(Player player, Map map)
    {
        if (map.RoomIs(RoomType.Amarock, player.Row, player.Column))
        {
            Console.WriteLine(player.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Amarock crushed you.");
            Console.WriteLine("You lost...");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        return false;
    }
}
public class Fountain // knows if the fountain is active; has the logic to activate it
{
    public bool IsActivated { get; private set; }
    public bool TryActivating(Map map, Player player)
    {
        if (map.RoomIs(RoomType.FountainOfObjects, player.Row, player.Column) && IsActivated == false)
        {
            IsActivated = true;
            return true;
        }
        return false;
    }
}
public interface ICommand 
{ 
    public void Run(Player player, Map map); 
    public static void ShootEnemy(Map map, int row, int column)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        if (map.RoomIs(RoomType.Amarock, row, column))
        {
            map.ClearRoom(row, column);
            Console.WriteLine("You shot down an amarock.");
        }
        else if (map.RoomIs(RoomType.Maelstrom, row, column))
        {
            map.ClearRoom(row, column);
            Console.WriteLine("You shot down a maelstrom.");
        }
        Console.ForegroundColor = ConsoleColor.White;
    }
}
public class MoveNorth : ICommand
{
    public void Run(Player player, Map map)
    {
        if (map.MoveValidation(player.Row - 1, player.Column))
            player.ChangeCoordinates(player.Row - 1, player.Column);
    }
}
public class MoveSouth : ICommand
{
    public void Run(Player player, Map map)
    {
        if (map.MoveValidation(player.Row + 1, player.Column))
            player.ChangeCoordinates(player.Row + 1, player.Column);
    }
}
public class MoveWest : ICommand
{
    public void Run(Player player, Map map)
    {
        if (map.MoveValidation(player.Row, player.Column - 1))
            player.ChangeCoordinates(player.Row, player.Column - 1);
    }
}
public class MoveEast : ICommand
{
    public void Run(Player player, Map map)
    {
        if (map.MoveValidation(player.Row, player.Column + 1))
            player.ChangeCoordinates(player.Row, player.Column + 1);
    }
}
public class ShootNorth : ICommand
{
    public void Run(Player player, Map map)
    {
        if (player.Arrows == 0)
        {
            Console.WriteLine("You are out of arrows.");
            return;
        }
        else if (player.Row - 1 > 0)
            ICommand.ShootEnemy(map, player.Row - 1, player.Column);
        else
            Console.WriteLine("Arrow hit the wall.");
        player.ArrowDecrement();
    }
}
public class ShootSouth : ICommand
{
    public void Run(Player player, Map map)
    {
        if (player.Arrows == 0)
        {
            Console.WriteLine("You are out of arrows.");
            return;
        }
        else if (player.Row + 1 < map.Room.GetLength(0))
            ICommand.ShootEnemy(map, player.Row + 1, player.Column);
        else
            Console.WriteLine("Arrow hit the wall.");
        player.ArrowDecrement();
    }
}
public class ShootWest : ICommand
{
    public void Run(Player player, Map map)
    {
        if (player.Arrows == 0)
        {
            Console.WriteLine("You are out of arrows.");
            return;
        }
        else if (player.Column - 1 > 0)
            ICommand.ShootEnemy(map, player.Row, player.Column - 1);
        else
            Console.WriteLine("Arrow hit the wall.");
        player.ArrowDecrement();
    }
}
public class ShootEast : ICommand
{
    public void Run(Player player, Map map)
    {
        if (player.Arrows == 0)
        {
            Console.WriteLine("You are out of arrows.");
            return;
        }
        else if (player.Column + 1 < map.Room.GetLength(1))
            ICommand.ShootEnemy(map, player.Row, player.Column + 1);
        else
            Console.WriteLine("Arrow hit the wall.");
        player.ArrowDecrement();
    }
}
public enum RoomType { Empty, Entrance, Pit, Maelstrom, Amarock, FountainOfObjects }