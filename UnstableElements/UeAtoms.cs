using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;

namespace UnstableElements;

using QuintessenceAtomColours = class_229;

using AtomTypes = class_175;
using Texture = class_256;

internal static class UeAtoms{
	
	public static AtomType Aether, Uranium;

	private static readonly List<AtomType> UraniumIsotopes = new(), SlowShakingIso = new(), FastShakingIso = new();

	private static readonly AtomTypeEq AtomComparator = new();
	private static readonly Random UraniumShakeCounter = new();
	private static ILHook SimValidationHook;
	private static Hook AetherBlockerHook;

	public static void AddAtomTypes(){
		// Aether atom type
		Aether = new(){
			field_2283 = 64, /*ID*/
			field_2284 = class_134.method_254("Aether"), /*Non-local Name*/
			field_2285 = class_134.method_253("Elemental Aether", string.Empty), /*Atomic Name*/
			field_2286 = class_134.method_253("Aether", string.Empty), /*Local name*/
			field_2287 = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_symbol"), /*Symbol*/
			field_2288 = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_shadow") /*Shadow*/
		};
		QuintessenceAtomColours aetherColours = new(){
			field_1950 = class_238.field_1989.field_81.field_613.field_627, /*Base*/
			field_1951 = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_colors"), /*Colours*/
			field_1952 = class_238.field_1989.field_81.field_613.field_629, /*Mask*/
			field_1953 = class_238.field_1989.field_81.field_613.field_630 /*Rimlight*/
		};
		Aether.field_2292 = aetherColours;
		Aether.field_2296 /*Non-metal?*/ = true;
		Aether.QuintAtomType = "UnstableElements:aether";

		QApi.AddAtomType(Aether);

		// Uranium atom types
		for(int phase = 0; phase < 3; phase++)
			for(int turn = 0; turn < 2; turn++){
				AtomType isotope = new(){
					field_2283 = 65,
					field_2284 = class_134.method_254("Uranium"),
					field_2285 = class_134.method_253("Elemental Uranium", string.Empty),
					field_2286 = class_134.method_253("Uranium", string.Empty),
					field_2287 = class_235.method_615($"textures/atoms/leppa/UnstableElements/uranium_symbol_{phase}"),
					field_2288 = class_238.field_1989.field_81.field_599,
					field_2291 = new(){
						field_13 = class_238.field_1989.field_81.field_577, /*Diffuse*/
						field_14 = class_235.method_615($"textures/atoms/leppa/UnstableElements/uranium_lightramp_{phase}"), /*Lightramp*/
						field_15 = class_238.field_1989.field_81.field_601 /*Rimlight*/
					}, /*Colours*/
					field_2294 = true /*Metal*/
				};
				if(phase == 0 && turn == 0){
					isotope.QuintAtomType = "UnstableElements:uranium";
					Uranium = isotope;
				}else
					isotope.QuintAtomType = $"UnstableElements:uranium:{phase}_{turn}";
				UraniumIsotopes.Add(isotope);
				if(phase == 1)
					SlowShakingIso.Add(isotope);
				else if(phase == 2)
					FastShakingIso.Add(isotope);
			}
		
		QApi.AddAtomType(Uranium);

		// Aether self-destruction
		QApi.RunAfterCycle((sim, first) => {
			if(!first){
				List<Molecule> toRemove = new();
				var molecules = sim.field_3823;
				foreach(var molecule in molecules){
					bool hasAether = false, hasNonAether = false;
					foreach(KeyValuePair<HexIndex, Atom> atom in molecule.method_1100())
						if(atom.Value.field_2275.Equals(Aether)){
							if(!UeParts.TranquilityHexes.Contains(atom.Key))
								hasAether = true;
						}else
							hasNonAether = true;

					if(hasAether && !hasNonAether)
						toRemove.Add(molecule);
				}

				foreach(var it in toRemove){
					foreach(KeyValuePair<HexIndex, Atom> atom in it.method_1100()){
						var seb = sim.field_3818;
						seb.field_3936.Add(new class_228(seb, (enum_7)1, class_187.field_1742.method_492(atom.Key) + new Vector2(80f, 0.0f), class_238.field_1989.field_90.field_240 /* or 42? */, 30f, Vector2.Zero, 0.0f));
					}

					molecules.Remove(it);
				}
			}
		});

		// Uranium heating
		QApi.RunAfterCycle((sim, first) => {
			if(first) return;
			
			var seb = sim.field_3818;
			var molecules = sim.field_3823;
			foreach(var molecule in molecules)
				foreach(KeyValuePair<HexIndex, Atom> atom in molecule.method_1100())
					if(!UeParts.TranquilityHexes.Contains(atom.Key))
						for(var idx = 0; idx < UraniumIsotopes.Count; idx++)
							if(atom.Value.field_2275.QuintAtomType == UraniumIsotopes[idx].QuintAtomType){
								if(idx == UraniumIsotopes.Count - 1)
									DoUraniumDecay(molecule, atom.Value, atom.Key, seb);
								else
									atom.Value.field_2275 = UraniumIsotopes[idx + 1];
								break;
							}
		});

		// Uranium visuals (shaking, heating)
		On.Editor.method_927 += OnAtomRender;
		// Molecule editor warning for pure-aether atoms
		On.MoleculeEditorScreen.method_50 += OnMoleculeEditorRender;
		// Shaking uranium validation
		SimValidationHook = new(typeof(Sim).GetMethod("method_1844", BindingFlags.NonPublic | BindingFlags.Static), ModSimValidate);
		// Blocking unstable pure-aether inputs
		AetherBlockerHook = new(typeof(Sim).GetMethod("method_1837", BindingFlags.NonPublic | BindingFlags.Instance), CheckInputProduction);
	}

	public static void Unload(){
		On.Editor.method_927 -= OnAtomRender;
		On.MoleculeEditorScreen.method_50 -= OnMoleculeEditorRender;
		SimValidationHook.Dispose();
		AetherBlockerHook.Dispose();
	}

	public static void DoUraniumDecay(Molecule m, Atom u, HexIndex pos, SolutionEditorBase seb){
		AtomType from = u.field_2275;
		m.method_1106(AtomTypes.field_1681, pos);
		u.field_2276 = new class_168(seb, 0, (enum_132)1, from, class_238.field_1989.field_81.field_614, 30f);
	}

	public static bool IsUraniumState(AtomType type) => UraniumIsotopes.Contains(type, AtomComparator);

	private static void OnAtomRender(On.Editor.orig_method_927 orig, AtomType type, Vector2 position, float param_4582, float param_4583, float param_4584, float param_4585, float param_4586, float param_4587, Texture overrideShadow, Texture maskM, bool param_4590){
		if(SlowShakingIso.Contains(type, AtomComparator))
			position += new Vector2((UraniumShakeCounter.Next(9) - 4) / 4f, (UraniumShakeCounter.Next(9) - 4) / 4f);
		if(FastShakingIso.Contains(type, AtomComparator))
			position += new Vector2((UraniumShakeCounter.Next(9) - 4f) / 2f, (UraniumShakeCounter.Next(9) - 4) / 2f);
		orig(type, position, param_4582, param_4583, param_4584, param_4585, param_4586, param_4587, overrideShadow, maskM, param_4590);
	}

	private static void ModSimValidate(ILContext il){
		ILCursor cursor = new(il);
		while(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld("Atom", "field_2275"))){
			cursor.Remove();
			cursor.EmitDelegate<Func<Atom, AtomType>>(u => {
				AtomType type = u.field_2275;
				return IsUraniumState(type) ? Uranium : type;
			});
		}
	}

	private static void OnMoleculeEditorRender(On.MoleculeEditorScreen.orig_method_50 orig, MoleculeEditorScreen self, float param_4858){
		orig(self, param_4858);
		DynamicData selfData = new(self);
		// if there's no existing error...
		if(!selfData.Get<Maybe<LocString>>("field_2661").method_1085()){
			Molecule m = selfData.Get<Molecule>("field_2656");
			// and there are only a nonzero amount of Aether atoms...
			if(m.method_1100().Count > 0 && m.method_1100().Values.Select(u => u.field_2275).All(u => u.Equals(Aether))){
				// display a warning
				Vector2 sizeM = new Vector2(1516f, 922f);
				Vector2 centreM = (class_115.field_1433 / 2 - sizeM / 2 + new Vector2(-2f, -11f)).Rounded();
				class_140.method_317("WARNING: Pure-aether molecules require a Glyph of Tranquility to handle.", centreM + new Vector2(471f, 107f), 922, false, false);
			}
		}
	}

	public delegate bool orig_method_1837(Sim self, Molecule toCheck, HashSet<HexIndex> moleculeFootprint);

	public static bool CheckInputProduction(orig_method_1837 orig, Sim self, Molecule toCheck, HashSet<HexIndex> moleculeFootprint){
		bool blocked = orig(self, toCheck, moleculeFootprint);
		if(!blocked) // if its not blocked by collisions, but is made of Aether and not stabilized, block it
			if(toCheck.method_1100().Values.Any() && toCheck.method_1100().Values.Select(u => u.field_2275).All(u => u.Equals(Aether)))
				if(!toCheck.method_1100().Keys.All(UeParts.TranquilityHexes.Contains))
					return true;
		return blocked;
	}

	// TODO: fix properly in quintessential
	private class AtomTypeEq : IEqualityComparer<AtomType>{
		public bool Equals(AtomType x, AtomType y){
			return x.QuintAtomType == y.QuintAtomType;
		}

		public int GetHashCode(AtomType obj){
			return obj.QuintAtomType.GetHashCode();
		}
	}
}