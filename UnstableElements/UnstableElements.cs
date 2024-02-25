using Quintessential;

namespace UnstableElements;

public class UnstableElements : QuintessentialMod{

	public override void Load(){
		
	}

	public override void PostLoad(){
		
	}

	public override void Unload(){
		Atoms.Unload();
		Parts.Unload();
		Solitaire.Unload();
	}

	public override void LoadPuzzleContent(){
		Atoms.AddAtomTypes();
		Parts.AddPartTypes();
		Solitaire.Load();
	}
}
