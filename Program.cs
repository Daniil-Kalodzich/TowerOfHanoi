using System.Text;
int size = 10;
var puzzle = new TowerOfHanoi(size);
Console.WriteLine(puzzle);

puzzle.SolveFor(size);

class TowerOfHanoi
{
	// data stack, used to store 3 pillows and their respective values
	private readonly Stack<int>[] _data;
	// used when working with precalculated moves
	private Queue<(int, int)> _recursiveMoves;
	private Queue<(int, int)> _forMoves;

	private readonly int _size;

	public static int counter = 0;

	public TowerOfHanoi(int size)
	{
		_size = size;
		_recursiveMoves = null;
		_forMoves = null;
		// init 3 stacks with initial capacity equal to max count of elements in game
		_data = new Stack<int>[] { new(size), new(size), new(size) };

		// filling first (left) tower with values
		// as a result we will get tower with element with value 1 on top, and element with value _size at the bottom
		for (var i = size; i > 0; i--)
			_data[0].Push(i);
	}

	// preparation moves should be used in pair with apply method / compare
	public void PrepareMovesFor(int value, int from = 0, int to = 2)
	{
		_forMoves = new Queue<(int, int)>(2 << value);

		// for even sizes we would like to swap these two towers
		int intermediateTower = 3 - from - to;
		if (value % 2 == 0)
		{
			(intermediateTower, to) = (to, intermediateTower);
		}

		int movesCount = 2 << --value;

		for (int i = 1; i < movesCount; i++)
		{
			_forMoves.Enqueue((i % 3) switch
			{
				// depending on i % 3 of current operation we could get what move in theory can applied to tower
				// but it does not give correct move, it gives only indexes of towers to perform operation with
				// for example if we get combination (0,1) there are two options to perform : (1,0) and (0,1)
				0 => (to, intermediateTower),
				1 => (from, to),
				2 => (intermediateTower, from),
				_ => (-1, -1)
			});
		}
	}

	public void PrepareMovesRecursion(int value, int from = 0, int to = 2)
	{
		if (_recursiveMoves is null)
			_recursiveMoves = new Queue<(int, int)>(2 << value);

		if (value == 0) return;

		var intermideateTower = 3 - from - to;

		PrepareMovesRecursion(value - 1, from, intermideateTower);

		_recursiveMoves.Enqueue((from, to));

		PrepareMovesRecursion(value - 1, intermideateTower, to);
	}

	public void CompareSolutions()
	{
		/*
		 * this method is to perform comparison between moves that we get from recursion and for preparation methods 
		 */
		var sb = new StringBuilder();

		int index = 0;
		// move from for
		(int from, int to) move;
		// move from recursion
		(int from, int to) idealMove;

		var recursiveMoves = new Queue<(int, int)>(_recursiveMoves);
		var forMoves = new Queue<(int, int)>(_forMoves);
		// index 0 for storing count of equal moves
		// index 1 for storing moves when we need to swap
		var swapsCount = new int[2];
		Console.WriteLine($"Size {_size} :");
		int prevIndex = 0;

		while (recursiveMoves.TryDequeue(out move))
		{
			forMoves.TryDequeue(out idealMove);
			// counting moves
			swapsCount[move.Equals(idealMove) ? 0 : 1] += 1;
			index++;
		
			if (!move.Equals(idealMove))
			{
				// this will append char '1' or '0' depending on difference between current index and previous index of swapped moves
				sb.Append(-((index - prevIndex - 2) / 2 + 1 - 2));
				prevIndex = index;
			}
		}
		// try printing sb and will notice that it is a palindrome and missing last character to be perfect
		sb.Append('1');
		

		// trying to predict palindrome based on only size value of tower
		// this will give us list of indexes offsets on which we should swap move around
		// might be used to create ideal move list based on FOR approximation
		var testSb = new StringBuilder();
		testSb.Append(_size % 2 == 0 ? "11" : "101");
		int depth = _size / 2 - 1;
		Console.WriteLine($"Calculated depth is {depth}");
		do
		{
			testSb.Replace("1", "1~1");
			testSb.Replace("0", "10001");
			testSb.Replace("~", "0");
			depth--;
		} while (depth > 0);



		Console.WriteLine($"should {sb.Length} calculated {testSb.Length}");
		Console.WriteLine(sb.ToString().Equals(testSb.ToString()));
	}
	public void ApplyRecursionMoves()
	{
		while (_recursiveMoves.TryDequeue(out (int from, int to) move))
		{
			PerformMove(move.from, move.to);
		}
	}

	public void ApplyForMoves()
	{
		while (_forMoves.TryDequeue(out (int from, int to) move))
		{
			// this moves might need to be swapped, we are checking this by comparing will current move violate rules of the game
			if (MoveNotAllowed(move))
			{
				move = (move.to, move.from);
			}

			PerformMove(move.from, move.to);
		}
	}

	public void SolveRecursion(int value, int from = 0, int to = 2)
	{
		counter++;

		if (value == 1)
		{
			PerformMove(from, to);
			return;
		}

		var intermideateTower = 3 - from - to;

		SolveRecursion(value - 1, from, intermideateTower);

		PerformMove(from, to);

		SolveRecursion(value - 1, intermideateTower, to);
	}

	private void PerformMove(int from, int to)
	{
		_data[to].Push(_data[from].Pop());
		Console.WriteLine(ToString());
	}

	public void SolveFor(int value, int from = 0, int to = 2)
	{
		int intermediateTower = 3 - from - to;
		if (value % 2 == 0)
		{
			(intermediateTower, to) = (to, intermediateTower);
		}

		long movesCount = 2 << --value;

		(int from, int to) move = default;
		for (long i = 1; i < movesCount; i++)
		{
			move = (i % 3) switch
			{
				0 => (to, intermediateTower),
				1 => (from, to),
				2 => (intermediateTower, from),
				_ => (-1, -1)
			};

			if (MoveNotAllowed(move))
			{
				move = (move.to, move.from);
			}

			PerformMove(move.from, move.to);
		}
	}

	public override string ToString()
	{
		var sb = new StringBuilder();

		var data = new List<int>[3];

		for (var i = 0; i < 3; i++)
		{
			data[i] = new List<int>(Enumerable.Repeat(0, _size - _data[i].Count));
			data[i].AddRange(_data[i]);
		}

		for (var y = 0; y < _size; y++)
		{
			for (var x = 0; x < 3; x++)
			{
				var element = data[x][y];
				sb.Append('\t');
				sb.Append(new string(element != 0 ? '*' : '|', element != 0 ? element : 1).PadLeft(_size));
				sb.Append(new string(element != 0 ? '*' : '|', element != 0 ? element : 1).PadRight(_size));

				sb.Append('\t');
			}

			sb.AppendLine();
		}

		sb.AppendLine();

		return sb.ToString();
	}

	private bool MoveNotAllowed((int from, int to) move)
	{
		return !_data[move.from].TryPeek(out var _) || (_data[move.from].TryPeek(out var valueFrom) && _data[move.to].TryPeek(out var valueTo) && valueFrom > valueTo);
	}
}
