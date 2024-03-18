using System;
using System.Collections.Generic;
using MonoMod.ModInterop;

namespace UnstableElements;

[ModExportName("UnstableElements")]
public class UeApi{

	// use with:
	// public static Action<Func<Sim, HashSet<HexIndex>>> RegisterStableHexesCallback;
	public static void RegisterStableHexesCallback(Func<Sim, HashSet<HexIndex>> cb){
		Parts.OtherStableHexesCallbacks.Add(cb);
	}
}