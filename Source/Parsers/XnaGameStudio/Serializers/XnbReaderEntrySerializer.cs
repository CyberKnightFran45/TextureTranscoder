using System.IO;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Serializer for XnbReaderInfo </summary>

public class XnbReaderEntrySerializer : IBinarySerializer<XnbReaderEntry>
{
// Read info

public XnbReaderEntry ReadBin(Stream reader)
{
string readerName = reader.ReadStringByVarLen();
int version = reader.ReadInt32();

return new(readerName, version);
}

// Write info 

public void WriteBin(Stream writer, XnbReaderEntry info)
{
writer.WriteStringByVarLen(info.ReaderName);

writer.WriteInt32(info.Version);
}

}

}