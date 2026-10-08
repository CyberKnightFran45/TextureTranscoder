using System;
using System.IO;

/// <summary> Logger for img parser </summary>

public static class TraceImgParser
{
// Parser executor

private static void ExecuteFileOp(TraceContext ctx,
                                  string inputPath,
								  ref string outputPath,
                                  string extension,
								  string message,
                                  Action<Stream, Stream> action)
{
PathHelper.ChangeExtension(ref outputPath, extension);

TraceFileSteps.Run(ctx,
                   inputPath,
				   outputPath,
				   message,
                   (i, o, _) => action(i, o) 
);

}

// Log encode process

public static void Encode(string operationName,
                          string inputPath,
						  string outputPath,
						  string extension,
                          Action<Stream, Stream> action,
                          params (string Name, object Value)[] args)
{

TraceExecutor.Run(operationName, ctx =>
{

ExecuteFileOp(ctx,
              inputPath,
			  ref outputPath,
              extension,
              "Encoding image...",
              action);
},

args
);

}

// Log decode process

public static void Decode(string operationName,
                          string inputPath,
						  string outputPath,
                          Action<Stream, Stream> action,
                          params (string Name, object Value)[] args)
{

TraceExecutor.Run(operationName, ctx =>
{

ExecuteFileOp(ctx,
              inputPath,
			  ref outputPath,
              ".png",
              "Decoding image...",
              action);
},

args
);	

}

}