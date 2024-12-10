Console.WriteLine("Do you want to play in small, medium or large game?");
string input = Console.ReadLine()!;
while (input != "small" && input != "medium" && input != "large")
{
    Console.WriteLine("That's not a valid option! Try again");
    input = Console.ReadLine()!;
}
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
    WorldEntity[] entities = [ new Maelstrom(), new Pit(), new Amarock() ];
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

            foreach (WorldEntity entity in entities)
                if (entity.Event(player, map))
                    return;
        }
        PlayerWon();
    }
    private void CommandExecution(string input)
    {
        ICommand? moveCommand = input switch
        {
            "move north" => new MoveCommand(player.Row - 1, player.Column),
            "move south" => new MoveCommand(player.Row + 1, player.Column),
            "move west" => new MoveCommand(player.Row, player.Column - 1),
            "move east" => new MoveCommand(player.Row, player.Column + 1),
            "shoot north" => new ShootCommand(player.Row - 1, player.Column),
            "shoot south" => new ShootCommand(player.Row + 1, player.Column),
            "shoot west" => new ShootCommand(player.Row, player.Column - 1),
            "shoot east" => new ShootCommand(player.Row + 1, player.Column),
            _ => null
        };
        if (moveCommand != null)
            moveCommand.Run(player, map);
        else if (input == "enable fountain")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (fountain.IsActivated)
                Console.WriteLine("You already activated the fountain!");
            else if (!fountain.Event(player, map))
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
public class WorldEntity
{
    protected string? Message { get; }
    protected RoomType RoomType { get; }
    public WorldEntity(RoomType roomType) { RoomType = roomType; }
    public WorldEntity(RoomType roomType, string message) { RoomType = roomType; Message = message; }
    public virtual bool Event(Player player, Map map)
    {
        if (map.RoomIs(RoomType, player.Row, player.Column))
        {
            Console.WriteLine(player.ToString());
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Message);
            Console.WriteLine("You lost...");
            Console.ForegroundColor = ConsoleColor.White;
            return true;
        }
        return false;
    }
}
public class Maelstrom : WorldEntity
{
    public Maelstrom() : base(RoomType.Maelstrom, "You were blown away by a Maelstrom.") { }
    public override bool Event(Player player, Map map)
    {
        if (map.Room[player.Row, player.Column] == RoomType)
        {
            map.Room[Math.Clamp(player.Row + 1, 0, map.Room.GetLength(0) - 1), Math.Clamp(player.Column - 2, 0, map.Room.GetLength(1) - 1)] = RoomType;
            map.Room[player.Row, player.Column] = RoomType.Empty;
            player.ChangeCoordinates(Math.Clamp(player.Row - 1, 0, map.Room.GetLength(0) - 1), Math.Clamp(player.Column + 2, 0, map.Room.GetLength(1) - 1));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(Message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return false;
    }
}
public class Pit : WorldEntity { public Pit() : base(RoomType.Pit, "You felt into a pit.") { } }
public class Amarock : WorldEntity { public Amarock() : base(RoomType.Amarock, "Amarock crushed you.") { } }
public class Fountain : WorldEntity
{
    public bool IsActivated { get; private set; }
    public Fountain() : base(RoomType.FountainOfObjects) { }
    public override bool Event(Player player, Map map)
    {
        if (map.RoomIs(RoomType, player.Row, player.Column) && IsActivated == false)
        {
            IsActivated = true;
            return true;
        }
        return false;
    }
}
// _row and _column fields below are the target coordinates
public interface ICommand { public void Run(Player player, Map map); }
public class MoveCommand : ICommand
{
    private readonly int _row;
    private readonly int _column;
    public MoveCommand(int row, int column) { _row = row; _column = column; }
    public void Run(Player player, Map map)
    {
        if (map.MoveValidation(_row, _column))
            player.ChangeCoordinates(_row, _column);
    }
}
public class ShootCommand : ICommand
{
    private readonly int _row;
    private readonly int _column;
    public ShootCommand(int row, int column) { _row = row; _column = column; }
    public void Run(Player player, Map map)
    {
        if (player.Arrows == 0)
        {
            Console.WriteLine("You are out of arrows.");
            return;
        }
        else if (_row < map.Room.GetLength(0) && _column < map.Room.GetLength(1) && _row >= 0 && _column >= 0)
            ShootEnemy(map);
        else
            Console.WriteLine("Arrow hit the wall.");
        player.ArrowDecrement();
    }
    private void ShootEnemy(Map map)
    {
        if (map.RoomIs(RoomType.Amarock, _row, _column))
            EnemyDown("You shot down an amarock.");
        else if (map.RoomIs(RoomType.Maelstrom, _row, _column))
            EnemyDown("You shot down a maelstrom.");
        else
            Console.WriteLine("Arrow didn't hit anything.");
        void EnemyDown(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            map.ClearRoom(_row, _column);
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
public enum RoomType { Empty, Entrance, Pit, Maelstrom, Amarock, FountainOfObjects }