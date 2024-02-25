using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;

namespace UnstableElements;

using Texture = class_256;
using AtomTypes = class_175;

public class Solitaire{

	// TODO: just run hookgen with private methods on please
	private static Hook hookJournalEntryRender, hookSolitaireStateGetter, hookSolitaireStateSetter;

	private static Texture sigmarSprite, sigmarHoverSprite;
	private static HexIndex[] indicies = new DynamicData(typeof(SolitaireScreen)).Get<HexIndex[]>("field_3867");
	
	// current solitaire state
	public static SolitaireState UeSolitaireState;

	// element placement
	public static List<AtomType> Cardinals = new(){
		AtomTypes.field_1675, // salt
		AtomTypes.field_1676, // air
		AtomTypes.field_1677, // earth
		AtomTypes.field_1678, // fire
		AtomTypes.field_1679  // water
	};
	public static List<AtomType> Metals = new(){
		AtomTypes.field_1685, // silver
		AtomTypes.field_1682, // copper
		AtomTypes.field_1684, // iron
		AtomTypes.field_1683, // tin
		AtomTypes.field_1681  // lead
	};
	public static AtomType Quicksilver = AtomTypes.field_1680;
	public static AtomType Gold = AtomTypes.field_1686;

	internal static void Load(){
		On.class_198.method_537 += OnGenerateSolitaireBoard;
		
		hookJournalEntryRender = new Hook(
			typeof(JournalScreen).GetMethod("method_1040", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnJournalEntryRender", BindingFlags.Static | BindingFlags.NonPublic)
		);
		hookSolitaireStateGetter = new Hook(
			typeof(SolitaireScreen).GetMethod("method_1889", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnSolitaireScreenGetState", BindingFlags.Static | BindingFlags.NonPublic)
		);
		hookSolitaireStateSetter = new Hook(
			typeof(SolitaireScreen).GetMethod("method_1890", BindingFlags.Instance | BindingFlags.NonPublic),
			typeof(Solitaire).GetMethod("OnSolitaireScreenSetState", BindingFlags.Static | BindingFlags.NonPublic)
		);

		sigmarSprite = class_235.method_615("UeJournal/sigmar");
		sigmarHoverSprite = class_235.method_615("UeJournal/sigmar_hover");
	}

	internal static void Unload(){
		On.class_198.method_537 -= OnGenerateSolitaireBoard;
		
		hookJournalEntryRender?.Dispose();
		hookSolitaireStateGetter?.Dispose();
		hookSolitaireStateSetter?.Dispose();
	}

	private static SolitaireGameState GenerateSolitaireBoard(){
		SolitaireGameState state = new(){
			field_3864 = { // gold in the centre
				[new HexIndex(5, 0)] = Gold
			}
		};
		
		// generate via a series of valid moves
		// go for marbles + metals
		Random rng = new();
		int curMetal = 0;
		int[] cardinalsPlaced = new int[Cardinals.Count];
		int aethers = 0;
		while(true){
			List<AtomType> choices = new(6);
			// could choose a cardinal that we don't have enough of
			for(var idx = 0; idx < cardinalsPlaced.Length; idx++)
				if(cardinalsPlaced[idx] < 6)
					choices.Add(Cardinals[idx]);
			// could choose a metal, if we have any left
			if(curMetal < Metals.Count)
				choices.Add(Metals[curMetal]);
			// could choose aether; higher priority to hopefully give them enough space
			if(aethers < 6){
				choices.Add(Atoms.Aether);
				choices.Add(Atoms.Aether);
			}

			if(choices.Count == 0)
				break; // we're done!

			AtomType next = rng.Choose(choices);
			if(next == Atoms.Aether){
				HexIndex pos = RandomFree(state, null, rng, threshold: 6);
				state.field_3864[pos] = next;
				aethers++;
			}else{
				HexIndex pos = RandomFree(state, null, rng);
				HexIndex pos2 = RandomFree(state, pos, rng);
				if(Cardinals.Contains(next)){
					state.field_3864[pos] = next;
					state.field_3864[pos2] = next;
					cardinalsPlaced[Cardinals.IndexOf(next)] += 2;
				}else{
					state.field_3864[pos] = next;
					state.field_3864[pos2] = Quicksilver;
					curMetal++;
				}
			}
		}

		return state;
	}

	private static HexIndex RandomFree(SolitaireGameState current, HexIndex? exclude, Random rng, int threshold = 3){
		if(exclude != null){
			current = current.method_1880();
			current.field_3864[exclude.Value] = AtomTypes.field_1675;
		}

		return rng.ChooseOrElse(indicies.Where(v => IsValidPlacement(v, current, threshold)).ToList(), new HexIndex(0, 0));
	}
	
	private static bool IsValidPlacement(HexIndex pos, SolitaireGameState self, int threshold){
		if(self.field_3864.ContainsKey(pos))
			return false;

		int currentBlanks = 0;
		int maxBlanks = 0;
		for(int index = 0; index < 2; ++index){
			foreach(HexIndex adjacentOffset in HexIndex.AdjacentOffsets)
				if(self.field_3864.ContainsKey(pos + adjacentOffset))
					currentBlanks = 0;
				else{
					++currentBlanks;
					maxBlanks = Math.Max(maxBlanks, currentBlanks);
				}
		}

		return maxBlanks >= threshold;
	}
	
	private static SolitaireGameState OnGenerateSolitaireBoard(On.class_198.orig_method_537 orig, bool quint){
		return SolitaireExt.IsCurrentSolitaireUe() ? GenerateSolitaireBoard() : orig(quint);
	}
	
	private delegate void orig_method_1040(JournalScreen self, Puzzle puzzle, Vector2 pos, bool big);
	private static void OnJournalEntryRender(orig_method_1040 orig, JournalScreen self, Puzzle puzzle, Vector2 pos, bool big){
		if(puzzle.field_2766 == "QuickIron"){
			Texture puzzleBg = big ? class_238.field_1989.field_88.field_894 : class_238.field_1989.field_88.field_895;
			class_256 tick = true /* TODO: count wins */ ? class_238.field_1989.field_96.field_879 : class_238.field_1989.field_96.field_882;
			class_256 divider = big ? class_238.field_1989.field_88.field_892 : class_238.field_1989.field_88.field_893;
			Bounds2 bounds = Bounds2.WithSize(pos, puzzleBg.field_2056.ToVector2());
			bool hover = bounds.Contains(Input.MousePos());
			class_135.method_290("Shattered Garden", pos + new Vector2(9, -19), class_238.field_1990.field_2144, class_181.field_1718, 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
			UI.DrawTexture(tick, pos + new Vector2(puzzleBg.field_2056.X - 27, -23f));
			UI.DrawTexture(puzzleBg, pos);
			UI.DrawTexture(divider, pos + new Vector2(7f, -34f));
			UI.DrawTexture(hover ? sigmarHoverSprite : sigmarSprite, bounds.Min + new Vector2(13f, 13f));
			if(hover && Input.IsLeftClickPressed()){
				var solitaireScreen = new SolitaireScreen(true);
				solitaireScreen.SetUe(true);
				UI.OpenScreen(solitaireScreen);
				class_238.field_1991.field_1821.method_28(1f);
			}
		}else
			orig(self, puzzle, pos, big);
	}

	private static SolitaireState OnSolitaireScreenGetState(Func<SolitaireScreen, SolitaireState> orig, SolitaireScreen self)
		=> self.IsUe() ? UeSolitaireState : orig(self);

	private static void OnSolitaireScreenSetState(Action<SolitaireScreen, SolitaireState> orig, SolitaireScreen self, SolitaireState next){
		if(self.IsUe())
			UeSolitaireState = next;
		else orig(self, next);
	}
}

internal static class SolitaireExt{

	private const string ueTag = "UnstableElements";
	
	internal static bool IsUe(this SolitaireScreen screen)
		=> DynamicData.For(screen).TryGet(ueTag, out bool? t) && t == true;

	internal static void SetUe(this SolitaireScreen screen, bool value)
		=> DynamicData.For(screen).Set(ueTag, value);

	internal static bool IsCurrentSolitaireUe()
		=> GameLogic.field_2434.method_938() is SolitaireScreen screen && screen.IsUe();
}

internal static class RandomExt{

	public static T Choose<T>(this Random rng, List<T> from) => from[rng.Next(from.Count)];

	public static T ChooseOrElse<T>(this Random rng, List<T> from, T fallback) => from.Count > 0 ? rng.Choose(from) : fallback;
}