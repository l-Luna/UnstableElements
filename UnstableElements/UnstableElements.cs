using Quintessential;

namespace UnstableElements;

public class UnstableElements : QuintessentialMod{

	public override void Load(){
		
	}

	public override void PostLoad(){
		
	}

	public override void Unload(){
		UeAtoms.Unload();
		UeParts.Unload();
	}

	public override void LoadPuzzleContent(){
		UeAtoms.AddAtomTypes();
		UeParts.AddPartTypes();
	}
}
