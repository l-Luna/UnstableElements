using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

using Quintessential;

namespace UnstableElements;

using MetalAtomColours = class_8;
using QuintessenceAtomColours = class_229;

using AtomTypes = class_175;
using Texture = class_256;

internal class UeAtoms {

	public static AtomType Aether, Uranium;

	private static AtomType UraniumT2, UShaking1, UShaking1T2, UShaking2, UShaking2T2;
	private static Dictionary<AtomType, AtomType> Heating;
	
	private static readonly Random UraniumShakeCounter = new();
	private static ILHook SimValidationHook;

	public static void AddAtomTypes(){
		// Aether atom type
		Aether = new();
		Aether.field_2283/*ID*/ = 64; // TODO: remove byte ID
		Aether.field_2284/*Non-local Name*/ = class_134.method_254("Aether");
		Aether.field_2285/*Atomic Name*/ = class_134.method_253("Elemental Aether", string.Empty);
		Aether.field_2286/*Local name*/ = class_134.method_253("Aether", string.Empty);
		Aether.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_symbol");
		Aether.field_2288/*Shadow*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_shadow");
		QuintessenceAtomColours aetherColours = new();
		aetherColours.field_1950/*Base*/ = class_238.field_1989.field_81.field_613.field_627;
		aetherColours.field_1951/*Colours*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/aether_colors");
		aetherColours.field_1952/*Mask*/ = class_238.field_1989.field_81.field_613.field_629;
		aetherColours.field_1953/*Rimlight*/ = class_238.field_1989.field_81.field_613.field_630;
		Aether.field_2292 = aetherColours;
		Aether.field_2296/*Non-metal?*/ = true;
		Aether.QuintAtomType = "UnstableElements:aether";

		// Uranium atom type
		Uranium = new();
		Uranium.field_2283/*ID*/ = 65; // TODO: remove byte ID
		Uranium.field_2284/*Non-local Name*/ = class_134.method_254("Uranium");
		Uranium.field_2285/*Atomic Name*/ = class_134.method_253("Elemental Uranium", string.Empty);
		Uranium.field_2286/*Local name*/ = class_134.method_253("Uranium", string.Empty);
		Uranium.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_symbol");
		Uranium.field_2288/*Shadow*/ = class_238.field_1989.field_81.field_599;
		MetalAtomColours uraniumColours = new();
		uraniumColours.field_13/*Diffuse*/ = class_238.field_1989.field_81.field_577;
		uraniumColours.field_14/*Lightramp*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_lightramp");
		uraniumColours.field_15/*Rimlight*/ = class_238.field_1989.field_81.field_601;
		Uranium.field_2291 = uraniumColours;
		Uranium.field_2294/*Metal?*/ = true;
		Uranium.QuintAtomType = "UnstableElements:uranium";

		QApi.AddAtomType(Aether);
		QApi.AddAtomType(Uranium);

		// Uranium shaking atom types
		UraniumT2 = SimpleCloneMetalType(Uranium, u => u.QuintAtomType = "UnstableElements:uranium:stable_turn_2");

		UShaking1 = SimpleCloneMetalType(UraniumT2, u => {
			u.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_x2_symbol");
			u.field_2291/*Metal colours*/ = new() {
				field_13/*Diffuse*/ = u.field_2291.field_13,
				field_14/*Lightramp*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_x2_lightramp"),
				field_15/*Rimlight*/ = u.field_2291.field_15
			};
			u.QuintAtomType = "UnstableElements:uranium:shaking_turn_1";
		});
		UShaking1T2 = SimpleCloneMetalType(UShaking1, u => u.QuintAtomType = "UnstableElements:uranium:shaking_turn_2");

		UShaking2 = SimpleCloneMetalType(UShaking1T2, u => {
			u.field_2287/*Symbol*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_x3_symbol");
			u.field_2291/*Metal colours*/ = new(){
				field_13/*Diffuse*/ = u.field_2291.field_13,
				field_14/*Lightramp*/ = class_235.method_615("textures/atoms/leppa/UnstableElements/uranium_x3_lightramp"),
				field_15/*Rimlight*/ = u.field_2291.field_15
			};
			u.QuintAtomType = "UnstableElements:uranium:shaking_2";
		});
		UShaking2T2 = SimpleCloneMetalType(UShaking2, u => u.QuintAtomType = "UnstableElements:uranium:shaking_2_turn_2");


		Heating = new(new AtomTypeEq()){
			{Uranium, UraniumT2},
			{UraniumT2, UShaking1},
			{UShaking1, UShaking1T2},
			{UShaking1T2, UShaking2},
			{UShaking2, UShaking2T2}
		};

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

		// Uranium heating
		QApi.RunAfterCycle((sim, first) => {
			if(!first) {
				var simData = new DynamicData(sim);
				var seb = simData.Get<SolutionEditorBase>("field_3818");
				var molecules = simData.Get<List<Molecule>>("field_3823");
				foreach(var molecule in molecules) {
					foreach(var atom in molecule.method_1100()){
						var type = atom.Value.field_2275;
						if(!UeParts.TranquilityHexes.Contains(atom.Key)){
							if(Heating.ContainsKey(type)){
								atom.Value.field_2275 = Heating[type];
							} else if(type == UShaking2T2){
								DoUraniumDecay(molecule, atom.Value, atom.Key, seb);
							}
						}
					}
				}
			}
		});

		// Uranium visuals (shaking, heating)
		On.Editor.method_927 += OnAtomRender;
		// Shaking uranium validation
		SimValidationHook = new(typeof(Sim).GetMethod("method_1844", BindingFlags.NonPublic | BindingFlags.Static), ModSimValidate);
	}

	public static void Unload(){
		On.Editor.method_927 -= OnAtomRender;
		SimValidationHook.Dispose();
	}

	public static void DoUraniumDecay(Molecule m, Atom u, HexIndex pos, SolutionEditorBase seb){
		m.method_1106(AtomTypes.field_1681, pos);
		u.field_2276 = new class_168(seb, 0, (enum_132)1, UShaking2T2, class_238.field_1989.field_81.field_614, 30f);
	}

	public static bool IsUraniumState(AtomType type){
		return type == Uranium || type == UraniumT2
			|| type == UShaking1 || type == UShaking1T2
			|| type == UShaking2 || type == UShaking2T2;
	}

	private static void OnAtomRender(On.Editor.orig_method_927 orig, AtomType type, Vector2 position, float param_4582, float param_4583, float param_4584, float param_4585, float param_4586, float param_4587, Texture overrideShadow, Texture maskM, bool param_4590){
		if(type == UShaking1 || type == UShaking1T2)
			position += new Vector2((UraniumShakeCounter.Next(9) - 4) / 4f, (UraniumShakeCounter.Next(9) - 4) / 4f);
		if(type == UShaking2 || type == UShaking2T2)
			position += new Vector2((UraniumShakeCounter.Next(9) - 4f) / 2f, (UraniumShakeCounter.Next(9) - 4) / 2f);
		orig(type, position, param_4582, param_4583, param_4584, param_4585, param_4586, param_4587, overrideShadow, maskM, param_4590);
	}

	private static void ModSimValidate(ILContext il){
		ILCursor cursor = new(il);
		while(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld("Atom", "field_2275"))){
			cursor.Remove();
			cursor.EmitDelegate<Func<Atom, AtomType>>(u => {
				AtomType type = u.field_2275;
				if(IsUraniumState(type))
					return Uranium;
				return type;
			});
		}
	}

	private static AtomType SimpleCloneMetalType(AtomType from, Action<AtomType> diff){
		AtomType copy = new(){
			field_2283/*ID*/ = (byte)(from.field_2283 + 1),
			field_2287/*Symbol*/ = from.field_2287,
			field_2288/*Shadow*/ = from.field_2288,
			field_2291/*Metal colours*/ = new() {
				field_13/*Diffuse*/ = from.field_2291.field_13,
				field_14/*Lightramp*/ = from.field_2291.field_14,
				field_15/*Rimlight*/ = from.field_2291.field_15
			},
			field_2294/*Metal?*/ = from.field_2294,
			QuintAtomType = from.QuintAtomType
		};
		diff(copy);
		return copy;
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