[System.Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3(UnityEngine.Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}

[System.Serializable]
public class SpawnPathData
{
    public SerializableVector3 spawnPoint;
    public System.Collections.Generic.List<System.Collections.Generic.List<SerializableVector3>> paths;

    public SpawnPathData()
    {
        paths = new System.Collections.Generic.List<System.Collections.Generic.List<SerializableVector3>>();
    }
}

[System.Serializable]
public class LevelPathData
{
    public System.Collections.Generic.List<SpawnPathData> spawnPaths;

    public LevelPathData()
    {
        spawnPaths = new System.Collections.Generic.List<SpawnPathData>();
    }
}