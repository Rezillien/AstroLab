public struct AABB
{
    public int minX;
    public int minY;
    public int maxX;
    public int maxY;

    public AABB(int minX, int minY, int maxX, int maxY)
    {
        this.minX = minX;
        this.minY = minY;
        this.maxX = maxX;
        this.maxY = maxY;
    }

    public int width()
    {
        return maxX - minX;
    }

    public int height()
    {
        return maxY - minY;
    }
}

