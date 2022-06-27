using System.Linq;
using System.Collections.Generic;

using MonoMod.Utils;

using Quintessential;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Data;

namespace UnstableElements;

using PartType = class_139;
using PartTypes = class_191;
using Permissions = enum_149;

using AtomTypes = class_175;

using Texture = class_256;

internal class UeParts{

	public static PartType Irradiation, Volatility, Tranquility;

    public static Texture IrradiationBase = class_235.method_615("textures/parts/leppa/UnstableElements/irradiation_base");
    public static Texture IrradiationGoldSymbol = class_235.method_615("textures/parts/leppa/UnstableElements/gold_symbol");
    public static Texture IrradiationMetalBowl = class_235.method_615("textures/parts/leppa/UnstableElements/irradiation_bowl");

    public static Texture VolatilitySymbol = class_235.method_615("textures/parts/leppa/UnstableElements/volatility_symbol");
    public static Texture VolatilityBowl = class_235.method_615("textures/parts/leppa/UnstableElements/volatility_bowl");

    public static Texture TranquilityBase = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility_base");
    public static Texture TranquilityQuicksilverSymbol = class_235.method_615("textures/parts/leppa/UnstableElements/quicksilver_symbol");
    public static Texture TranquilityMetalBowl = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility_bowl");
    public static Texture TranquilityProjectors = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility_projectors");
    public static Texture TranquilityZoneHex = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility_zone_hex");
    public static Color TranquilityZoneColor = new(255 / 255f, 251 / 255f, 199 / 255f, 255 / 255f);

    private static readonly string TranquilityPowerId = "UnstableElements:tranquility_powered";
    private static readonly HashSet<HexIndex> TranquilityOffsets = new(){
        new(1, -1),
        new(1, -2), new(2, -2),
        new(1, -3), new(2, -3), new(3, -3),
        new(1, -4), new(2, -4), new(3, -4), new(4, -4)
    };

    public static readonly HashSet<HexIndex> TranquilityHexes = new();

    public static void AddPartTypes(){
        Irradiation = new(){
            field_1528 = "unstable-elements-irradiation", // ID
            field_1529 = class_134.method_253("Glyph of Irradiation", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of irradiation projects an atom of gold into an unstable atom of uranium.", string.Empty), // Description
            field_1531 = 25, // Cost
            field_1539 = true, // Is a glyph (?)
            field_1549 = class_238.field_1989.field_97.field_384, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_385, // Stroke/outline
            field_1547 = class_235.method_615("textures/parts/leppa/UnstableElements/irradiation"), // Panel icon
            field_1548 = class_235.method_615("textures/parts/leppa/UnstableElements/irradiation_hovered"), // Hovered panel icon
            field_1540 = new HexIndex[4]{
              new HexIndex(0, 0),
              new HexIndex(-1, 1),
              new HexIndex(1, 0),
              new HexIndex(0, -1)
            }, // Spaces used
            field_1551 = Permissions.None,
            //CustomPermissionCheck = perms => perms.Contains("unstable-elements-irradiation")
        };

        Volatility = new(){
            field_1528 = "unstable-elements-volatility", // ID
            field_1529 = class_134.method_253("Glyph of Volatility", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of volatility causes an atom of uranium to instantly decay, regardless of its heat.", string.Empty), // Description
            field_1531 = 10, // Cost
            field_1539 = true, // Is a glyph (?)
            field_1549 = class_238.field_1989.field_97.field_382, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_383, // Stroke/outline
            field_1547 = class_235.method_615("textures/parts/leppa/UnstableElements/volatility"), // Panel icon
            field_1548 = class_235.method_615("textures/parts/leppa/UnstableElements/volatility_hovered"), // Hovered panel icon
            field_1540 = new HexIndex[1]{
              new HexIndex(0, 0)
            }, // Spaces used
            field_1551 = Permissions.None,
            //CustomPermissionCheck = perms => perms.Contains("unstable-elements-volatility")
        };

        Tranquility = new(){
            field_1528 = "unstable-elements-tranquility", // ID
            field_1529 = class_134.method_253("Glyph of Tranquility", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of tranquility projects a field that stabilizes uranium and aether atoms, preventing their decays.", string.Empty), // Description
            field_1531 = 40, // Cost
            field_1539 = true, // Is a glyph (?)
            field_1549 = class_238.field_1989.field_97.field_386, // Shadow/glow
            field_1550 = class_238.field_1989.field_97.field_387, // Stroke/outline
            field_1547 = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility"), // Panel icon
            field_1548 = class_235.method_615("textures/parts/leppa/UnstableElements/tranquility_hovered"), // Hovered panel icon
            field_1540 = new HexIndex[3]{
              new HexIndex(0, 0),
              new HexIndex(1, 0),
              new HexIndex(0, 1)
            }, // Spaces used
            field_1551 = Permissions.None,
            //CustomPermissionCheck = perms => perms.Contains("unstable-elements-tranquility")
        };

        QApi.AddPartType(Irradiation, (part, pos, editor, renderer) => {
            Vector2 vector2 = new(83f, 119f);
            renderer.method_523(IrradiationBase, new Vector2(0.0f, -1f), vector2, 0.0f);
            foreach(HexIndex idx in part.method_1159().field_1540){
                if(idx.Q == 0 && idx.R == 0){
                    renderer.method_530(class_238.field_1989.field_90.field_164 /*bonder_shadow*/, idx, 0);
                    renderer.method_528(IrradiationMetalBowl, idx, Vector2.Zero);
                    renderer.method_529(IrradiationGoldSymbol, idx, Vector2.Zero);
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
        QApi.AddPartType(Volatility, (part, pos, editor, renderer) => {
			Texture calcinatorBase = class_238.field_1989.field_90.field_169;
            Vector2 centre = (calcinatorBase.field_2056.ToVector2() / 2).Rounded() + new Vector2(0, 1);
            renderer.method_521(calcinatorBase, centre);
            renderer.method_530(class_238.field_1989.field_90.field_228.field_273 /* ring_shadow */, new HexIndex(0, 0), 3);
            renderer.method_528(VolatilityBowl, new HexIndex(0, 0), Vector2.Zero);
            renderer.method_521(VolatilitySymbol, centre);
        });
        QApi.AddPartType(Tranquility, (part, pos, editor, renderer) => {
            Vector2 vector2 = new(42, 48);
            renderer.method_523(TranquilityBase, new Vector2(-1, -1), vector2, 0);
            HexIndex qsSite = new(0, 1);
            renderer.method_530(class_238.field_1989.field_90.field_164 /*bonder_shadow*/, qsSite, 0);
            renderer.method_528(TranquilityMetalBowl, qsSite, Vector2.Zero);
            renderer.method_529(TranquilityQuicksilverSymbol, qsSite, Vector2.Zero);
			
            double time = System.Math.Sin(new struct_27(Time.Now().Ticks).method_603());
            float pulse = (float)(time / 3 + .66);
            if(editor.method_503() != enum_128.Stopped && new DynamicData(part).TryGet(TranquilityPowerId, out bool? power) && power == true){
                Color tint = Color.White;
                tint.A *= pulse;
                DrawForPartWithTint(renderer, TranquilityProjectors, new Vector2(-1, -1), vector2, 0, tint);
			}
		});

        QApi.AddPartTypeToPanel(Irradiation, PartTypes.field_1775);
        QApi.AddPartTypeToPanel(Volatility, PartTypes.field_1775);
        QApi.AddPartTypeToPanel(Tranquility, PartTypes.field_1775);
        //QApi.AddPuzzlePermission("unstable-elements-irradiation", "Glyph of Irradiation");
        //QApi.AddPuzzlePermission("unstable-elements-volatility", "Glyph of Volatility");
        //QApi.AddPuzzlePermission("unstable-elements-tranquility", "Glyph of Tranquility");

        QApi.RunAfterCycle((_, _) => {
            // first thing
            TranquilityHexes.Clear();
        });

        QApi.RunAfterCycle((sim, first) => {
            var simData = new DynamicData(sim);
            var seb = simData.Get<SolutionEditorBase>("field_3818");
            var allParts = simData.Invoke<Solution>("method_1817").field_3919;
            var partsAndGrippers = new List<Part>(allParts);
            partsAndGrippers.AddRange(allParts.SelectMany(u => u.field_2696));
            
            foreach(var part in allParts){
                var type = part.method_1159();
                // look for 3 unheld QSs and free gold
                if(type == Irradiation){
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
                }else if(type == Volatility){
                    if(FindAtom(simData, part, new HexIndex(0, 0), partsAndGrippers).method_99(out AtomReference uranium))
                        if(UeAtoms.IsUraniumState(uranium.field_2280))
                            UeAtoms.DoUraniumDecay(uranium.field_2277, uranium.field_2279, uranium.field_2278, seb);
                }else if(type == Tranquility){
                    bool isPowered =
                        FindAtom(simData, part, new HexIndex(0, 1), partsAndGrippers).method_99(out AtomReference qs)
                            && qs.field_2280 == AtomTypes.field_1680; // is QS
                    new DynamicData(part).Set(TranquilityPowerId, isPowered);
					if(isPowered){
						foreach(var offset in TranquilityOffsets){
                            var adjusted = part.method_1184(offset);
                            TranquilityHexes.Add(adjusted);
						}
					}
                }
			}
        });

		On.SolutionEditorBase.method_1984 += DrawTranquilityField;
    }

	public static void Unload(){
        On.SolutionEditorBase.method_1984 -= DrawTranquilityField;
    }

    private static Maybe<AtomReference> FindAtom(DynamicData simData, Part self, HexIndex offset, List<Part> allParts) {
        HexIndex position = self.method_1184(offset);
        foreach(Molecule molecule in simData.Get<List<Molecule>>("field_3823")){
            if(molecule.method_1100().TryGetValue(position, out Atom atom)){
                bool flag = false;
                foreach(Part part in allParts){
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

    private static void DrawForPartWithTint(class_195 renderer, Texture tex, Vector2 offset, Vector2 size, float rotation, Color c){
        Matrix4 tf = Matrix4.method_1070((renderer.field_1797 + offset).ToVector3(0)) * Matrix4.method_1073(renderer.field_1798 + rotation) * Matrix4.method_1070(-size.ToVector3(0)) * Matrix4.method_1074(tex.field_2056.ToVector3(0));
        class_135.method_262(tex, c, tf);
    }

    private static void DrawTranquilityField(On.SolutionEditorBase.orig_method_1984 orig, SolutionEditorBase self, Vector2 param_5533, Bounds2 param_5534, Bounds2 param_5535, bool param_5536, Maybe<List<Molecule>> param_5537, bool param_5538) {
        orig(self, param_5533, param_5534, param_5535, param_5536, param_5537, param_5538);

		if(self.method_503() != enum_128.Stopped) {
            double time = System.Math.Sin(new struct_27(Time.Now().Ticks).method_603());
            float pulse = (float)(time / 4 + .75) / 2.4f;

            Color tint = TranquilityZoneColor;
            class_187 conv = class_187.field_1742;
            tint.A *= pulse;
            foreach(var hex in TranquilityHexes){
                Vector2 hexAsVec = conv.method_492(hex) + param_5533 - new Vector2(2, 8);
                Matrix4 tf = Matrix4.method_1070(hexAsVec.ToVector3(0)) * Matrix4.method_1073(0) * Matrix4.method_1070(new Vector3(-40, -40, 0)) * Matrix4.method_1074(TranquilityZoneHex.field_2056.ToVector3(0));
                class_135.method_262(TranquilityZoneHex, tint, tf);
            }
        }
    }
}
