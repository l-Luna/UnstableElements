using System.Collections.Generic;

using MonoMod.Utils;

using Quintessential;

namespace UnstableElements;

using MetalAtomColours = class_8;
using QuintessenceAtomColours = class_229;

internal class UeAtoms {

	public static AtomType Aether, Uranium;


	public static void AddAtomTypes() {
		// Aether atom type
		AtomType aether = new();
		aether.field_2283/*ID*/ = 64; // TODO: remove byte ID
		aether.field_2284/*Non-local Name*/ = class_134.method_254("Aether");
		aether.field_2285/*Atomic Name*/ = class_134.method_253("Elemental Aether", string.Empty);
		aether.field_2286/*Local name*/ = class_134.method_253("Aether", string.Empty);
		aether.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_symbol");
		aether.field_2288/*Shadow*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_shadow");
		QuintessenceAtomColours aetherColours = new();
		aetherColours.field_1950/*Base*/ = class_238.field_1989.field_81.field_613.field_627;
		aetherColours.field_1951/*Colours*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_colors");
		aetherColours.field_1952/*Mask*/ = class_238.field_1989.field_81.field_613.field_629;
		aetherColours.field_1953/*Rimlight*/ = class_238.field_1989.field_81.field_613.field_630;
		aether.field_2292 = aetherColours;
		aether.field_2296/*Non-metal?*/ = true;
		aether.QuintAtomType = "UnstableElements:aether";
		Aether = aether;

		// Uranium atom type
		AtomType uranium = new();
		uranium.field_2283/*ID*/ = 65; // TODO: remove byte ID
		uranium.field_2284/*Non-local Name*/ = class_134.method_254("Uranium");
		uranium.field_2285/*Atomic Name*/ = class_134.method_253("Elemental Uranium", string.Empty);
		uranium.field_2286/*Local name*/ = class_134.method_253("Uranium", string.Empty);
		uranium.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_symbol");
		uranium.field_2288/*Shadow*/ = class_238.field_1989.field_81.field_599;
		MetalAtomColours uraniumColours = new();
		uraniumColours.field_13/*Diffuse*/ = class_238.field_1989.field_81.field_577;
		uraniumColours.field_14/*Lightramp*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_lightramp");
		uraniumColours.field_15/*Rimlight*/ = class_238.field_1989.field_81.field_601;
		uranium.field_2291 = uraniumColours;
		uranium.field_2294/*Metal?*/ = true;
		uranium.QuintAtomType = "UnstableElements:uranium";
		Uranium = uranium;

		QApi.AddAtomType(Aether);
		QApi.AddAtomType(Uranium);

		// Aether self-destruction
		QApi.RunAfterCycle((sim, first) => {
			if(!first) {
				List<Molecule> toRemove = new List<Molecule>();
				var molecules = new DynamicData(sim).Get<List<Molecule>>("field_3823");
				foreach(var molecule in molecules) {
					bool hasAether = false, hasNonAether = false;
					foreach(var atom in molecule.method_1100())
						if(atom.Value.field_2275.Equals(Aether))
							hasAether = true;
						else
							hasNonAether = true;
					if(hasAether && !hasNonAether)
						toRemove.Add(molecule);
				}
				foreach(var it in toRemove) {
					foreach(var atom in it.method_1100()) {
						var seb = new DynamicData(sim).Get<SolutionEditorBase>("field_3818");
						seb.field_3936.Add(new class_228(seb, (enum_7)1, class_187.field_1742.method_492(atom.Key) + new Vector2(80f, 0.0f), class_238.field_1989.field_90.field_240 /* or 42? */, 30f, Vector2.Zero, 0.0f));
					}
					molecules.Remove(it);
				}
			}
		});
	}
}