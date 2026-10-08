namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Represents a XNB reader entry </summary>

public class XnbReaderEntry
{
/// <summary> Reader name </summary>

public string ReaderName{ get; set; }

/// <summary> Reader version </summary>

public int Version{ get; set; }

// ctor

public XnbReaderEntry()
{
}

// ctor 2

public XnbReaderEntry(string name, int version)
{
ReaderName = name;
Version = version;
}

}

}