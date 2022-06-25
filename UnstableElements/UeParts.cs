﻿using System.Linq;
using MonoMod.Utils;

using System.Collections.Generic;

using Quintessential;

namespace UnstableElements;

using PartType = class_139;
using PartTypes = class_191;
using Permissions = enum_149;

using AtomTypes = class_175;

using Texture = class_256;

internal class UeParts{

	public static PartType Volatility;

    public static Texture VolatilityBase = class_235.method_615("textures/parts/leppa/UnstableElements/volatility_base");
    public static Texture VolatilityGoldSymbol = class_235.method_615("textures/parts/leppa/UnstableElements/gold_symbol");
    public static Texture VolatilityMetalBowl = class_235.method_615("textures/parts/leppa/UnstableElements/metal_bowl");

    public static void AddPartTypes(){
        Volatility = new(){
            field_1528 = "unstable-elements-volatility", // ID
            field_1529 = class_134.method_253("Glyph of Volatility", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of volatility projects an atom of gold into an unstable atom of uranium.", string.Empty), // Description
            field_1531 = 25, // Cost
            field_1539 = true, // Is a glyph (?)
            field_1549 = class_238.field_1989.field_97.field_384, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_385, // Stroke/outline
            field_1547 = class_235.method_615("textures/parts/leppa/UnstableElements/volatility"), // Panel icon
            field_1548 = class_235.method_615("textures/parts/leppa/UnstableElements/volatility_hovered"), // Hovered panel icon
            field_1540 = new HexIndex[4]{
              new HexIndex(0, 0),
              new HexIndex(-1, 1),
              new HexIndex(1, 0),
              new HexIndex(0, -1)
            }, // Spaces used
            field_1551 = Permissions.None,
            //CustomPermissionCheck = perms => perms.Contains("unstable-elements-volatility")
        };

        QApi.AddPartType(Volatility, (part, pos, editor, renderer) => {
            Vector2 vector2 = new(83f, 119f);
            renderer.method_523(VolatilityBase, new Vector2(0.0f, -1f), vector2, 0.0f);
            foreach(HexIndex idx in part.method_1159().field_1540){
                if(idx.Q == 0 && idx.R == 0){
                    renderer.method_530(class_238.field_1989.field_90.field_164 /*bonder_shadow*/, idx, 0);
                    renderer.method_528(VolatilityMetalBowl, idx, Vector2.Zero);
                    renderer.method_529(VolatilityGoldSymbol, idx, Vector2.Zero);
                }else{
                    renderer.method_530(class_238.field_1989.field_90.field_164 /*bonder_shadow*/, idx, 0);
                    renderer.method_530(class_238.field_1989.field_90.field_255.field_293 /*quicksilver_input*/, idx, 0);
                    // should be 272?
                    renderer.method_529(class_238.field_1989.field_90.field_255.field_294 /*quicksilver_symbol*/, idx, Vector2.Zero);
                }
            }
            for(int i = 0; i < part.method_1159().field_1540.Length; i++){
                HexIndex hexIndex = part.method_1159().field_1540[i];
                if(hexIndex != new HexIndex(0, 0)){
                    int index = i - 1;
                    float num = new HexRotation(index * 2).ToRadians();
                    renderer.method_522(class_238.field_1989.field_90.field_255.field_289 /*bond*/, new Vector2(-30f, 12f), num);
                }
            }
        });

        QApi.AddPartTypeToPanel(Volatility, PartTypes.field_1775);
        //QApi.AddPuzzlePermission("unstable-elements-volatility", "Glyph of Volatility");

        QApi.RunAfterCycle((sim, first) => {
            // look for 3 unheld QSs and free gold
            var simData = new DynamicData(sim);
            var seb = simData.Get<SolutionEditorBase>("field_3818");
            var allParts = simData.Invoke<Solution>("method_1817").field_3919;
            var partsAndGrippers = new List<Part>(allParts);
            partsAndGrippers.AddRange(allParts.SelectMany(u => u.field_2696));
            foreach(var part in allParts) {
				if(part.method_1159() == Volatility) {
                    // if all the atoms exist...
					if(FindAtom(simData, part, new HexIndex(0, 0), partsAndGrippers).method_99(out AtomReference gold)
                    && FindAtom(simData, part, new HexIndex(-1, 1), partsAndGrippers).method_99(out AtomReference qs1)
                    && FindAtom(simData, part, new HexIndex(1, 0), partsAndGrippers).method_99(out AtomReference qs2)
                    && FindAtom(simData, part, new HexIndex(0, -1), partsAndGrippers).method_99(out AtomReference qs3)){
						// and are the right types...
                        if(gold.field_2280 == AtomTypes.field_1686
                        && qs1.field_2280 == AtomTypes.field_1680
                        && qs2.field_2280 == AtomTypes.field_1680
                        && qs3.field_2280 == AtomTypes.field_1680){
                            // and the quicksilver is not being consumed or held...
							if(!qs1.field_2281 && !qs1.field_2282
                            && !qs2.field_2281 && !qs2.field_2282
                            && !qs3.field_2281 && !qs3.field_2282){
                                // transmutate the gold and destroy the quicksilver
                                gold.field_2277.method_1106(UeAtoms.Uranium, gold.field_2278);
                                qs1.field_2277.method_1107(qs1.field_2278);
                                qs2.field_2277.method_1107(qs2.field_2278);
                                qs3.field_2277.method_1107(qs3.field_2278);
                                // show the removal effects for qs
                                seb.field_3937.Add(new class_286(seb, qs1.field_2278, AtomTypes.field_1680));
                                seb.field_3937.Add(new class_286(seb, qs2.field_2278, AtomTypes.field_1680));
                                seb.field_3937.Add(new class_286(seb, qs3.field_2278, AtomTypes.field_1680));
                                // upgrade effect for gold -> uranium
                                gold.field_2279.field_2276 = new class_168(seb, 0, (enum_132)1, gold.field_2280, class_238.field_1989.field_81.field_614, 30f);
                                // glowy effect on central hex
                                class_187 conv = class_187.field_1742;
                                HexIndex pos = part.method_1161();
                                Vector2 posAsVec = conv.method_492(pos);
                                List<class_228> effects = seb.field_3935;
								Texture[] glowFrames = class_238.field_1989.field_90.field_256;
                                class_228 glowEffect = new(seb, (enum_7)1, posAsVec, glowFrames, 30f, Vector2.Zero, 0);
                                effects.Add(glowEffect);
                                class_238.field_1991.field_1844.method_28(seb.method_506());
                            }
						}
					}
				}
			}
        });
    }

    private static Maybe<AtomReference> FindAtom(DynamicData simData, Part self, HexIndex offset, List<Part> allParts) {
        HexIndex position = self.method_1184(offset);
        foreach(Molecule molecule in simData.Get<List<Molecule>>("field_3823")){
            if(molecule.method_1100().TryGetValue(position, out Atom atom)) {
                bool flag = false;
                foreach(Part part in allParts) {
                    if(simData.Get<Dictionary<Part, PartSimState>>("field_3821")[part].field_2724 == position){
                        flag = true;
                        break;
                    }
                }
                return new AtomReference(molecule, position, atom.field_2275, atom, flag);
            }
        }
        return struct_18.field_1431;
    }
}
