using Godot;
using System; 
 
public class ElementIterator : System.Collections.IEnumerable {
    
    private Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int,Element>> elements;
    private int mode;

    public const int NORMAL = 0;
    public const int REVERSE = 1;
    public const int REVERSE_X = 2;
    public const int REVERSE_Y = 3;

    private int max;
    private int min;
    public ElementIterator(Godot.Collections.Dictionary<int, Godot.Collections.Dictionary<int,Element>> e, int max = 3, int min = 0)
    {
        this.elements = e;
        this.mode = NORMAL;
        this.max = max;
        this.min = min;
    }
    public System.Collections.IEnumerator GetEnumerator()
    {
        if (this.mode == NORMAL)
        {
            for (int x = min; x <= max; x++)
            {
                for (int y = min; y <= max; y++)
                {
                    yield return elements[x][y];
                }
            }
        }
        else if (this.mode == REVERSE)
        {
            for (int x = max; x >= min; x--)
            {
                for (int y = max; y >= min; y--)
                {
                    yield return elements[x][y];
                }
            }
        }
        else if (this.mode == REVERSE_X)
        {
            for (int x = max; x >= min; x--)
            {
                for (int y = min; y <= max; y++)
                {
                    yield return elements[x][y];
                }
            }
        }
        else if (this.mode == REVERSE_Y)
        {
            for (int x = min; x <= max; x++)
            {
                for (int y = max; y >= min; y--)
                {
                    yield return elements[x][y];
                }
            }
        }
    }

    public void SetMode(int mode)
    {
        if (mode != NORMAL && mode != REVERSE && mode != REVERSE_X && mode != REVERSE_Y)
        {
            throw new Exception("invalid mode");
        }
        this.mode = mode;
    }

    public int GetMode()
    {
        return this.mode;
    }

    public int CountEmpties()
    {
        int count = 0;
        foreach (Element e in this){
            if (e.Empty()) count++;
        }
        return count;
    }
}