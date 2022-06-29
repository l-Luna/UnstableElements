using System.Linq;
using System.Collections.Generic;

using MonoMod.Utils;

using Quintessential;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Data;
using System;

namespace UnstableElements;

using PartType = class_139;
using PartTypes = class_191;
using Permissions = enum_149;

using AtomTypes = class_175;

using Texture = class_256;

internal class UeParts{

    // TODO: move FindAtom and HeldGrippers to Quintessential

	public static PartType Irradiation, Volatility, Tranquility, Sublimation;

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

    public static Texture SublimationBelowIris = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation_below_iris");
    public static Texture SublimationAboveIris = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation_above_iris");
    public static Texture SublimationQuintessenceSymbol = class_235.method_615("textures/parts/leppa/UnstableElements/quintessence_symbol");
    public static Texture[] SublimationAetherIris = new Texture[16];
    public static Texture[] SublimationSaltIris = new Texture[16];

    public static readonly HashSet<HexIndex> TranquilityHexes = new();

    private static readonly string TranquilityPowerId = "UnstableElements:tranquility_powered";
    private static readonly HashSet<HexIndex> TranquilityOffsets = new(){
        new(1, -1),
        new(1, -2), new(2, -2),
        new(1, -3), new(2, -3), new(3, -3),
        new(1, -4), new(2, -4), new(3, -4), new(4, -4)
    };

    // these get reset at the start of a cycle after being collected by vanilla
    private static List<Part> HeldGrippers;
    private static Hook FindHeldGrippersHook;

    public static void AddPartTypes(){
		for(int i = 0; i < 16; i++) {
            SublimationAetherIris[i] = class_235.method_615($"textures/parts/leppa/UnstableElements/iris_full_aether.array/iris_full_00{i + 1 :D2}");
            SublimationSaltIris[i] = class_235.method_615($"textures/parts/leppa/UnstableElements/iris_full_salt.array/iris_full_00{i + 1:D2}");
        }

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

        Sublimation = new(){
            field_1528 = "unstable-elements-sublimation", // ID
            field_1529 = class_134.method_253("Glyph of Sublimation", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of sublimation splits an atom of quintessence into two molecules of stabilized aether.", string.Empty), // Description
            field_1531 = 10, // Cost
            field_1539 = true, // Is a glyph (?)
            field_1549 = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation_glow"), // Shadow/glow
            field_1550 = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation_stroke"), // Stroke/outline
            field_1547 = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation"), // Panel icon
            field_1548 = class_235.method_615("textures/parts/leppa/UnstableElements/sublimation_hovered"), // Hovered panel icon
            field_1540 = new HexIndex[5]{
              new HexIndex(0, 0),
              new HexIndex(0, 1),
              new HexIndex(1, 1),
              new HexIndex(0, -1),
              new HexIndex(-1, -1)
            }, // Spaces used
            field_1551 = Permissions.None,
            //CustomPermissionCheck = perms => perms.Contains("unstable-elements-sublimation")
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
        QApi.AddPartType(Sublimation, (part, pos, editor, renderer) => {
            PartSimState myState = editor.method_507().method_481(part);
            Vector2 vector2 = new(330 / 2, 238 / 2);
            var renderInfo = editor.method_1989(part, pos);

            // base
            renderer.method_523(SublimationBelowIris, new Vector2(-1, -1), vector2, 0);

            // centre hex
            renderer.method_529(SublimationQuintessenceSymbol, new(0, 0), new(3, 3));
			if(myState.field_2743) // disappearing quintessence for active glyph
                Editor.method_925(Molecule.method_1121(AtomTypes.field_1690), RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, 1f - editor.method_504(), 1f, false, null);

            Molecule stabilizedAether = new();
            stabilizedAether.method_1105(new Atom(UeAtoms.Aether), new(1, 1));
            stabilizedAether.method_1105(new Atom(AtomTypes.field_1675), new(0, 1));
            stabilizedAether.method_1112(enum_126.Standard, new(0, 1), new(1, 1), struct_18.field_1431);
            Molecule stbAetherRot = new();
            stbAetherRot.method_1105(new Atom(UeAtoms.Aether), new(-1, -1));
            stbAetherRot.method_1105(new Atom(AtomTypes.field_1675), new(0, -1));
            stbAetherRot.method_1112(enum_126.Standard, new(0, -1), new(-1, -1), struct_18.field_1431);

            // irises
            int animIdx = 15;
            float prog = 0;
			if(myState.field_2743){
                animIdx = class_162.method_404((int)((double)class_162.method_411(1f, -1f, editor.method_504()) * 16), 0, 15);
                prog = editor.method_504();
            }
            if(prog < 0.5){ // render under irises
				Editor.method_925(stabilizedAether, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, prog, 1f, false, null);
                Editor.method_925(stbAetherRot, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, prog, 1f, false, null);
            }
            renderer.method_529(SublimationSaltIris[animIdx], new(0, 1), new(2, 0));
            renderer.method_529(SublimationSaltIris[animIdx], new(0, -1), new(2, 0));
            renderer.method_529(SublimationAetherIris[animIdx], new(1, 1), new(2, 0));
            renderer.method_529(SublimationAetherIris[animIdx], new(-1, -1), new(2, 0));
			if(prog > 0.5){ // render over irises
                Editor.method_925(stabilizedAether, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, prog, 1f, false, null);
                Editor.method_925(stbAetherRot, RelativeToGlobal(renderInfo, new(0, 0)), new(0, 0), 0.0f, 1f, prog, 1f, false, null);
            }

            // top
            renderer.method_523(SublimationAboveIris, new Vector2(-1, -1), vector2, 0);
        });

        QApi.AddPartTypeToPanel(Irradiation, PartTypes.field_1775);
        QApi.AddPartTypeToPanel(Volatility, PartTypes.field_1775);
        QApi.AddPartTypeToPanel(Tranquility, PartTypes.field_1775);
        QApi.AddPartTypeToPanel(Sublimation, PartTypes.field_1775);
        //QApi.AddPuzzlePermission("unstable-elements-irradiation", "Glyph of Irradiation");
        //QApi.AddPuzzlePermission("unstable-elements-volatility", "Glyph of Volatility");
        //QApi.AddPuzzlePermission("unstable-elements-tranquility", "Glyph of Tranquility");
        //QApi.AddPuzzlePermission("unstable-elements-sublimation", "Glyph of Sublimation");

        QApi.RunAfterCycle((_, _) => {
            // first thing
            TranquilityHexes.Clear();
        });

        QApi.RunAfterCycle((sim, first) => {
            var simData = new DynamicData(sim);
            var seb = simData.Get<SolutionEditorBase>("field_3818");
            var allParts = simData.Invoke<Solution>("method_1817").field_3919;
            var simStates = simData.Get<Dictionary<Part, PartSimState>>("field_3821");

            foreach(var part in allParts){
                var type = part.method_1159();
                // look for 3 unheld QSs and free gold
                if(type == Irradiation){
                    // if all the atoms exist...
                    if(FindAtom(simData, part, new HexIndex(0, 0), HeldGrippers).method_99(out AtomReference gold)
                    && FindAtom(simData, part, new HexIndex(-1, 1), HeldGrippers).method_99(out AtomReference qs1)
                    && FindAtom(simData, part, new HexIndex(1, 0), HeldGrippers).method_99(out AtomReference qs2)
                    && FindAtom(simData, part, new HexIndex(0, -1), HeldGrippers).method_99(out AtomReference qs3)){
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
                    if(FindAtom(simData, part, new HexIndex(0, 0), HeldGrippers).method_99(out AtomReference uranium))
                        if(UeAtoms.IsUraniumState(uranium.field_2280))
                            UeAtoms.DoUraniumDecay(uranium.field_2277, uranium.field_2279, uranium.field_2278, seb);
                }else if(type == Tranquility){
                    bool isPowered =
                        FindAtom(simData, part, new HexIndex(0, 1), HeldGrippers).method_99(out AtomReference qs)
                            && qs.field_2280 == AtomTypes.field_1680; // is QS
                    new DynamicData(part).Set(TranquilityPowerId, isPowered);
					if(isPowered){
						foreach(var offset in TranquilityOffsets){
                            var adjusted = part.method_1184(offset);
                            TranquilityHexes.Add(adjusted);
						}
					}
                }else if(type == Sublimation){
                    var mySimState = simStates[part];
                    // if we're in the accepting phase...
                    if(!mySimState.field_2743){
                        // if we have an unheld unbonded quintessence at the centre...
                        if(FindAtom(simData, part, new(0, 0), HeldGrippers).method_99(out AtomReference quint)
                            && quint.field_2280 == AtomTypes.field_1690
                            && !quint.field_2281 && !quint.field_2282){
                            // and no atoms are blocking our outputs...
                            if(!FindAtom(simData, part, new(1, 0), HeldGrippers).method_1085()
                                && !FindAtom(simData, part, new(1, 1), HeldGrippers).method_1085()
                                && !FindAtom(simData, part, new(0, -1), HeldGrippers).method_1085()
                                && !FindAtom(simData, part, new(-1, -1), HeldGrippers).method_1085()){
                                // destroy the quintessence
                                quint.field_2277.method_1107(quint.field_2278);
                                // set this part to be inactive the rest of the cycle
                                mySimState.field_2743 = true;
                                // play the production sound
                                class_238.field_1991.field_1841.method_28(seb.method_506());
                                // mark output positions as collidable
                                HexIndex[] outputs = new HexIndex[4]{
                                    new HexIndex(0, 1),
                                    new HexIndex(1, 1),
                                    new HexIndex(0, -1),
                                    new HexIndex(-1, -1)
                                };
                                var collisions = simData.Get<List<Sim.struct_122>>("field_3826");
								foreach(var hex in outputs){
                                    Vector2 vector2 = class_187.field_1742.method_491(part.method_1184(hex), Vector2.Zero);
                                    List<Sim.struct_122> field3826 = simData.Get<List<Sim.struct_122>>("field_3826");
                                    Sim.struct_122 collision = new(){
                                        field_3850 = 0,
                                        field_3851 = vector2,
                                        field_3852 = simData.Get<float>("field_3832")
                                    };
                                    collisions.Add(collision);
                                }
                            }
                        }
                    }else{
                        // otherwise, we're in the producing phase
                        Molecule stabilizedAether = new();
                        stabilizedAether.method_1105(new Atom(UeAtoms.Aether), part.method_1184(new HexIndex(1, 1)));
                        stabilizedAether.method_1105(new Atom(AtomTypes.field_1675), part.method_1184(new HexIndex(0, 1)));
                        stabilizedAether.method_1112(enum_126.Standard, part.method_1184(new HexIndex(0, 1)), part.method_1184(new HexIndex(1, 1)), struct_18.field_1431);
                        Molecule stbAetherRot = new();
                        stbAetherRot.method_1105(new Atom(UeAtoms.Aether), part.method_1184(new HexIndex(-1, -1)));
                        stbAetherRot.method_1105(new Atom(AtomTypes.field_1675), part.method_1184(new HexIndex(0, -1)));
                        stbAetherRot.method_1112(enum_126.Standard, part.method_1184(new HexIndex(0, -1)), part.method_1184(new HexIndex(-1, -1)), struct_18.field_1431);

                        var molecules = simData.Get<List<Molecule>>("field_3823");
                        molecules.Add(stabilizedAether);
                        molecules.Add(stbAetherRot);

                        mySimState.field_2743 = false;
                    }
                }
            }
        });

        FindHeldGrippersHook = new(
            typeof(Sim).GetMethod("method_1832", BindingFlags.NonPublic | BindingFlags.Instance),
            FindHeldGrippers
        );

		On.SolutionEditorBase.method_1984 += DrawTranquilityField;
    }

	public static void Unload(){
        FindHeldGrippersHook.Dispose();

        On.SolutionEditorBase.method_1984 -= DrawTranquilityField;
    }

    private static Maybe<AtomReference> FindAtom(DynamicData simData, Part self, HexIndex offset, List<Part> allParts){
        HexIndex position = self.method_1184(offset);
        var simStates = simData.Get<Dictionary<Part, PartSimState>>("field_3821");
        foreach(Molecule molecule in simData.Get<List<Molecule>>("field_3823")){
            if(molecule.method_1100().TryGetValue(position, out Atom atom)){
                bool flag = false;
                foreach(Part part in allParts){
                    if(simStates[part].field_2724 == position){
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

		if(self.method_503() != enum_128.Stopped){
            double time = Math.Sin(new struct_27(Time.Now().Ticks).method_603());
            float pulse = (float)(time / 4 + .75) / 2.4f;

			if(self is class_194)
                pulse = 0.25f; // constant brightness in GIFs

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

    private delegate void orig_method_1832(Sim self, bool first);
    private static void FindHeldGrippers(orig_method_1832 orig, Sim self, bool first){
        var simData = new DynamicData(self);
        var allParts = simData.Invoke<Solution>("method_1817").field_3919;
        var simStates = simData.Get<Dictionary<Part, PartSimState>>("field_3821");

        HeldGrippers = new();
		foreach(var part in allParts)
			foreach(var gripper in part.field_2696)
				if(simStates[gripper].field_2729.method_1085())
                    HeldGrippers.Add(gripper);

        orig(self, first);
    }

    private static Vector2 RelativeToGlobal(class_236 partRenderInfo, HexIndex pos){
        return partRenderInfo.field_1984 + class_187.field_1742.method_492(pos).Rotated(partRenderInfo.field_1985);
    }
}
