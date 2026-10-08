using System.IO;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Serializer for XnbReaderInfo </summary>

public class XnbReaderInfoSerializer : IBinarySerializer<XnbReaderInfo>
{
// ReaderEntry serializer

private static readonly XnbReaderEntrySerializer entrySerializer = new();

// Read entries

private static XnbReaderEntry[] ReadEntries(Stream reader)
{
uint readerCount = reader.ReadVarInt();

XnbReaderEntry[] typeReaders = new XnbReaderEntry[readerCount];

for(int i = 0; i < readerCount; i++)
typeReaders[i] = entrySerializer.ReadBin(reader);

return typeReaders;
}

// Read info

public XnbReaderInfo ReadBin(Stream reader)
{
var typeReaders = ReadEntries(reader);

uint sharedRes = reader.ReadVarInt();
uint primaryTypeId = reader.ReadVarInt();

return new(typeReaders, sharedRes, primaryTypeId);
}

// Write entries

private static void WriteEntries(Stream writer, XnbReaderEntry[] entries)
{
var readerCount = (uint)entries.Length;

writer.WriteVarInt(readerCount);

for(uint i = 0; i < readerCount; i++)
entrySerializer.WriteBin(writer, entries[i] );

}

// Write info 

public void WriteBin(Stream writer, XnbReaderInfo info)
{
WriteEntries(writer, info.TypeReaders);

writer.WriteVarInt(info.SharedRes);
writer.WriteVarInt(info.PrimaryTypeID);
}

}

}