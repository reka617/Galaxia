using System;

public enum Map
{
    Default
}

public enum GameMode
{
    Default
}

public enum GameQue
{
    Solo,
    Team
}

[Serializable]
public class UserData 
{
    public string userName;

    public string userAuthId;

    public GameInfo userGamePreferences;
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQue GameQue;

    internal string ToMultiplayQueue()
    {
        throw new NotImplementedException();
    }
}
