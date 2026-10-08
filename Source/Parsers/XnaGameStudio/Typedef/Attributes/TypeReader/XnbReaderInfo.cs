namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Info related to XNB reader </summary>

public class XnbReaderInfo
{
/// <summary> Readers </summary>

public XnbReaderEntry[] TypeReaders{ get; set; }

/// <summary> Amount of Shared resources </summary>

public uint SharedRes{ get; set; }

/// <summary> Primary Type ID </summary>

public uint PrimaryTypeID{ get; set; } = 1;

// ctor

public XnbReaderInfo()
{
}

/// ctor 2

public XnbReaderInfo(XnbReaderEntry[] readers, uint sharedRes, uint primaryID)
{
TypeReaders = readers;

SharedRes = sharedRes;
PrimaryTypeID = primaryID;
}

}

}