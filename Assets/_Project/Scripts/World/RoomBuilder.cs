using System.Collections.Generic;
using UnityEngine;

namespace Subject626
{
    public class RoomBuildResult
    {
        public Room room;
        public List<Light> roomLights = new List<Light>();
        public List<IExit> exits = new List<IExit>();
        public Transform backstageRoot;
        public Vector3 backstageSpawn;
        public float backstageYaw;
    }

    /// <summary>
    /// Arma la sala "decorada" con sus tres salidas y el backstage greybox del reveal.
    /// Todo por codigo con primitivas. Medidas: interior 10 (x) x 12 (z) x 4.2 (alto).
    /// </summary>
    public static class RoomBuilder
    {
        static PhysicsMaterial grip;

        public static RoomBuildResult Build()
        {
            RoomBuildResult res = new RoomBuildResult();

            GameObject rootGo = new GameObject("Room");
            Transform root = rootGo.transform;
            Room room = rootGo.AddComponent<Room>();
            res.room = room;

            grip = new PhysicsMaterial("grip");
            grip.dynamicFriction = 0.7f; grip.staticFriction = 0.8f;
            grip.frictionCombine = PhysicsMaterialCombine.Maximum;

            Material wall = MaterialLib.Solid(MaterialLib.WallPaper);
            Material floorMat = MaterialLib.Solid(MaterialLib.WoodFloor);
            Material ceilMat = MaterialLib.Solid(MaterialLib.Ceiling);

            const float HX = 5f, HZ = 6f, H = 4.2f, T = 0.3f;

            // Piso y techo.
            Prim.Box(root, "Floor", new Vector3(0f, -0.15f, 0f), new Vector3(HX * 2f + 0.6f, 0.3f, HZ * 2f + 0.6f), floorMat);
            Prim.Box(root, "Ceiling", new Vector3(0f, H + 0.15f, 0f), new Vector3(HX * 2f + 0.6f, 0.3f, HZ * 2f + 0.6f), ceilMat);

            // Pared izquierda solida.
            Prim.Box(root, "WallLeft", new Vector3(-HX - T * 0.5f, H * 0.5f, 0f), new Vector3(T, H, HZ * 2f + 0.6f), wall);

            // Pared del fondo (z-) con un HUECO para el panel rajable (x[3.2,4.8], y[0,2.4]).
            float bz = -HZ - T * 0.5f;
            Prim.Box(root, "WallBackMain", new Vector3(-1.05f, H * 0.5f, bz), new Vector3(8.5f, H, T), wall);       // izquierda del hueco
            Prim.Box(root, "WallBackTop", new Vector3(4.0f, 2.4f + (H - 2.4f) * 0.5f, bz), new Vector3(1.6f, H - 2.4f, T), wall); // sobre el hueco
            Prim.Box(root, "WallBackCorner", new Vector3(5.05f, H * 0.5f, bz), new Vector3(0.5f, H, T), wall);       // esquina derecha
            BuildBreakablePanel(root, new Vector3(4.0f, 1.2f, bz), new Vector3(1.6f, 2.4f, 0.28f));

            // Pared del frente (z+) con hueco de puerta (ancho 1.6, alto 2.6).
            float doorHalf = 0.8f, doorH = 2.6f;
            float segW = HX - doorHalf; // 4.2
            Prim.Box(root, "WallFrontL", new Vector3(-(doorHalf + segW * 0.5f), H * 0.5f, HZ + T * 0.5f), new Vector3(segW, H, T), wall);
            Prim.Box(root, "WallFrontR", new Vector3(doorHalf + segW * 0.5f, H * 0.5f, HZ + T * 0.5f), new Vector3(segW, H, T), wall);
            Prim.Box(root, "WallFrontTop", new Vector3(0f, doorH + (H - doorH) * 0.5f, HZ + T * 0.5f), new Vector3(doorHalf * 2f, H - doorH, T), wall);

            // Puerta (hoja) que tapa el hueco. Bisagra al borde izquierdo para que gire lindo.
            GameObject hinge = new GameObject("DoorHinge");
            hinge.transform.SetParent(root, false);
            hinge.transform.localPosition = new Vector3(-doorHalf + 0.02f, 0f, HZ);
            Material doorMat = MaterialLib.Solid(MaterialLib.DoorWood, 0.2f);
            GameObject leaf = Prim.Box(hinge.transform, "DoorLeaf", new Vector3(doorHalf - 0.02f, doorH * 0.5f, 0f), new Vector3(doorHalf * 2f - 0.06f, doorH, 0.12f), doorMat);
            // Picaporte.
            Prim.Sphere(leaf.transform, "Knob", new Vector3(0.55f, 0f, 0.6f), 0.14f, MaterialLib.Solid(MaterialLib.Gold, 0.7f, 0.8f), false);
            DoorController door = hinge.AddComponent<DoorController>();
            door.leaf = hinge.transform;
            res.exits.Add(door);

            // Pared DERECHA: se ve igual pero NO tiene collider (solucion "pared falsa").
            GameObject fakeWall = Prim.Box(root, "WallRight_FAKE", new Vector3(HX + T * 0.5f, H * 0.5f, 0f), new Vector3(T, H, HZ * 2f + 0.6f), wall, false);

            // Luces calidas de la sala (se ve terminada).
            res.roomLights.Add(Prim.PointLight(root, new Vector3(-2f, 3.6f, -2f), new Color(1f, 0.85f, 0.6f), 12f, 14f));
            res.roomLights.Add(Prim.PointLight(root, new Vector3(2f, 3.6f, 2.5f), new Color(1f, 0.82f, 0.55f), 12f, 14f));
            // Lampara colgante decorativa.
            Prim.Cyl(root, "LampCord", new Vector3(0f, 3.9f, 0f), 0.02f, 0.6f, MaterialLib.Solid(Color.black), false);
            Prim.Sphere(root, "LampShade", new Vector3(0f, 3.55f, 0f), 0.5f, MaterialLib.Emissive(new Color(1f, 0.9f, 0.7f), 1.2f), false);

            BuildDecor(root);
            BuildProps(root, room);
            if (KeySolutionEnabled) BuildHiddenKey(root, room);
            res.exits.Add(BuildCeilingPoster(root, H));
            res.exits.Add(BuildFalseWallExit(root, HX, HZ, fakeWall));
            res.exits.Add(BuildPressurePlate(root));

            // Codigo de 4 digitos (aleatorio por partida) para el teclado + pistas repartidas.
            string code = "" + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10) + Random.Range(0, 10);
            res.exits.Add(BuildKeypadAndClues(root, code, HZ));

            // El panel rajable se creo arriba (pared del fondo); lo registramos como salida.
            BreakablePanel bp = root.GetComponentInChildren<BreakablePanel>(true);
            if (bp != null) res.exits.Add(bp);

            // Entrada: contra la pared del fondo, mirando hacia adentro (+z).
            room.entrancePos = new Vector3(0f, 0.1f, -HZ + 1.2f);
            room.entranceYaw = 0f;

            // Backstage greybox (el reveal).
            BuildBackstage(res);

            return res;
        }

        // --- Decoracion estatica (vende que la sala esta "terminada") -----------
        static void BuildDecor(Transform root)
        {
            Material tableMat = MaterialLib.Solid(MaterialLib.CrateDark, 0.3f);
            GameObject table = Prim.Box(root, "TableTop", new Vector3(-3.2f, 1.0f, -3.5f), new Vector3(1.8f, 0.12f, 1.0f), tableMat);
            for (int i = 0; i < 4; i++)
            {
                float sx = (i % 2 == 0) ? -0.8f : 0.8f;
                float sz = (i < 2) ? -0.42f : 0.42f;
                Prim.Box(table.transform, "Leg", new Vector3(sx, -0.5f, sz), new Vector3(0.1f, 1.0f, 0.1f), tableMat);
            }

            // Cuadros en la pared del fondo.
            Prim.Box(root, "Frame1", new Vector3(-2.5f, 2.4f, -5.83f), new Vector3(1.0f, 0.7f, 0.06f), MaterialLib.Emissive(MaterialLib.Blue, 0.6f));
            Prim.Box(root, "Frame2", new Vector3(0.2f, 2.6f, -5.83f), new Vector3(0.8f, 1.1f, 0.06f), MaterialLib.Emissive(MaterialLib.Green, 0.6f));

            // Estanteria a la izquierda.
            Material shelf = MaterialLib.Solid(MaterialLib.CrateDark, 0.3f);
            GameObject sh = Prim.Box(root, "Shelf", new Vector3(-4.6f, 1.6f, 3.2f), new Vector3(0.4f, 3.0f, 2.2f), shelf);
            Prim.Box(sh.transform, "ShelfPlank1", new Vector3(0.2f, 0.2f, 0f), new Vector3(0.6f, 0.05f, 2.0f), shelf, false);
            Prim.Box(sh.transform, "ShelfPlank2", new Vector3(0.2f, -0.4f, 0f), new Vector3(0.6f, 0.05f, 2.0f), shelf, false);
        }

        // --- Objetos fisicos: cajas apilables + clutter + la caja con la llave ---
        static void BuildProps(Transform root, Room room)
        {
            // Cajas apilables (la solucion del poster: apilar y subirse).
            Vector3[] crates = new Vector3[]
            {
                new Vector3(-1.4f, 0.37f, 0.2f),
                new Vector3(-0.4f, 0.37f, -0.9f),
                new Vector3(0.7f, 0.37f, 0.6f),
                new Vector3(1.7f, 0.37f, -0.4f),
                new Vector3(-2.3f, 0.37f, -1.6f),
            };
            for (int i = 0; i < crates.Length; i++)
                room.grabbables.Add(MakeCrate(root, "Crate" + i, crates[i], 0.72f, MaterialLib.Crate, 2.6f));

            // Clutter decorativo tambien agarrable (sandbox: todo se levanta).
            room.grabbables.Add(MakeCrate(root, "SmallBox", new Vector3(3.2f, 0.22f, 2.2f), 0.42f, MaterialLib.CrateDark, 1.2f));
            room.grabbables.Add(MakeBall(root, "Ball", new Vector3(2.6f, 0.28f, -1.8f), 0.5f, MaterialLib.Blue, 0.8f));
            room.grabbables.Add(MakeBall(root, "Ball2", new Vector3(-3.4f, 0.24f, 1.0f), 0.42f, MaterialLib.Green, 0.6f));
        }

        static Grabbable MakeCrate(Transform root, string name, Vector3 pos, float size, Color color, float mass)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(size, size, size);
            go.GetComponent<Renderer>().sharedMaterial = MaterialLib.Solid(color, 0.15f);
            BoxCollider bc = go.GetComponent<BoxCollider>();
            bc.sharedMaterial = grip;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            return go.AddComponent<Grabbable>();
        }

        static Grabbable MakeBall(Transform root, string name, Vector3 pos, float diameter, Color color, float mass)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(root, false);
            go.transform.localPosition = pos;
            go.transform.localScale = new Vector3(diameter, diameter, diameter);
            go.GetComponent<Renderer>().sharedMaterial = MaterialLib.Solid(color, 0.4f);
            SphereCollider sc = go.GetComponent<SphereCollider>();
            sc.sharedMaterial = grip;
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = mass;
            return go.AddComponent<Grabbable>();
        }

        // --- Solucion "llave" (dificil): escondida DEBAJO de la alfombra, sin pistas --
        // Poner en false para SACARLA del todo: la puerta pasa a ser siempre trampa.
        public const bool KeySolutionEnabled = true;

        static void BuildHiddenKey(Transform root, Room room)
        {
            // La alfombra parece pura decoracion y esta medio metida bajo la mesa.
            Material rugMat = MaterialLib.Solid(MaterialLib.Red, 0.05f);
            GameObject rug = Prim.Box(root, "Rug", new Vector3(-2.7f, 0.02f, -2.9f), new Vector3(2.2f, 0.04f, 1.9f), rugMat);

            // Llave escondida abajo (inactiva hasta levantar la alfombra). Sin brillo escandaloso.
            GameObject keyGo = new GameObject("Key");
            keyGo.transform.SetParent(root, false);
            keyGo.transform.localPosition = new Vector3(-2.7f, 0.12f, -2.9f);
            Material keyMat = MaterialLib.Emissive(MaterialLib.Gold, 1.0f);
            Prim.Box(keyGo.transform, "KeyBit", new Vector3(0f, 0f, -0.18f), new Vector3(0.1f, 0.02f, 0.16f), keyMat, false);
            Prim.Cyl(keyGo.transform, "KeyShaft", new Vector3(0f, 0f, 0.02f), 0.03f, 0.4f, keyMat, false);
            Prim.Box(keyGo.transform, "KeyRing", new Vector3(0f, 0f, 0.22f), new Vector3(0.16f, 0.02f, 0.16f), keyMat, false);
            KeyItem key = keyGo.AddComponent<KeyItem>();
            keyGo.SetActive(false);

            RugCover cover = rug.AddComponent<RugCover>();
            cover.key = key;
            room.rug = cover;
            room.key = key;
        }

        // --- Solucion "poster del techo" ----------------------------------------
        static CeilingPoster BuildCeilingPoster(Transform root, float H)
        {
            // Poster pegado al techo, hacia el centro-fondo (hay lugar para apilar).
            GameObject posterGo = Prim.Box(root, "CeilingPoster", new Vector3(-0.6f, H - 0.06f, -1.2f), new Vector3(1.6f, 0.06f, 1.1f), MaterialLib.Solid(MaterialLib.Poster, 0.1f));
            // Conducto que se revela detras (empieza apagado).
            GameObject vent = Prim.Box(root, "Vent", new Vector3(-0.6f, H - 0.05f, -1.2f), new Vector3(1.5f, 0.5f, 1.0f), MaterialLib.Solid(Color.black));
            Prim.DestroySafe(vent.GetComponent<Collider>());
            vent.SetActive(false);
            CeilingPoster poster = posterGo.AddComponent<CeilingPoster>();
            poster.vent = vent.transform;
            return poster;
        }

        // --- Solucion "pared falsa": plataforma + hueco + salida ----------------
        static SealableWall BuildFalseWallExit(Transform root, float HX, float HZ, GameObject fakeWall)
        {
            Material plat = MaterialLib.Solid(MaterialLib.GreyDark, 0.2f);
            // Plataforma al otro lado del hueco (hay que saltar CON carrera; hueco ~2.5m).
            Prim.Box(root, "Platform", new Vector3(9.7f, -0.15f, 0f), new Vector3(3.8f, 0.3f, 6.3f), plat);
            // Marca de salida en la plataforma.
            Prim.Box(root, "ExitPad", new Vector3(9.7f, 0.02f, 0f), new Vector3(2.6f, 0.04f, 2.6f), MaterialLib.Emissive(MaterialLib.Green, 1.5f));

            // Volumen de salida sobre la plataforma.
            GameObject exitGo = MakeExit(root, "PlatformExit", new Vector3(9.7f, 1.2f, 0f), new Vector3(3.6f, 2.4f, 6.0f), ExitId.FalseWall, true);

            // Trigger del hueco: si caes, volves a la entrada.
            GameObject pitGo = new GameObject("Pit");
            pitGo.transform.SetParent(root, false);
            pitGo.transform.localPosition = new Vector3(6.7f, -2.0f, 0f);
            BoxCollider pc = pitGo.AddComponent<BoxCollider>();
            pc.isTrigger = true; pc.size = new Vector3(4.8f, 2.0f, 13f);
            pitGo.AddComponent<PitTrigger>();

            // Collider de la pared falsa (deshabilitado) para poder SELLARLA al usarla.
            BoxCollider wallCol = fakeWall.AddComponent<BoxCollider>();
            wallCol.enabled = false;
            SealableWall sw = fakeWall.AddComponent<SealableWall>();
            sw.wallCollider = wallCol;
            sw.platformExit = exitGo;
            return sw;
        }

        /// <summary>Crea un volumen de salida (trigger) que registra ese ExitId al pisarlo.</summary>
        static GameObject MakeExit(Transform parent, string name, Vector3 pos, Vector3 size, ExitId id, bool startActive)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            BoxCollider bc = go.AddComponent<BoxCollider>();
            bc.isTrigger = true; bc.size = size;
            ExitTrigger et = go.AddComponent<ExitTrigger>();
            et.id = id;
            go.SetActive(startActive);
            return go;
        }

        // --- Solucion "panel rajable": romperlo a los golpes de objetos lanzados --
        static void BuildBreakablePanel(Transform root, Vector3 center, Vector3 size)
        {
            GameObject panel = Prim.Box(root, "BreakablePanel", center, size, MaterialLib.Solid(MaterialLib.WallPaper, 0.1f));
            // Grieta visible para que se lea "debil".
            Prim.Box(panel.transform, "Crack", new Vector3(0f, 0f, -0.55f), new Vector3(0.1f, 0.9f, 0.1f), MaterialLib.Solid(new Color(0.1f, 0.1f, 0.1f)), false);

            // Repisa afuera + salida (detras de la pared, arranca sin trigger activo).
            Prim.Box(root, "BreakLedge", new Vector3(center.x, -0.15f, center.z - 0.9f), new Vector3(2.0f, 0.3f, 1.6f), MaterialLib.Solid(MaterialLib.GreyDark, 0.2f));
            Prim.Box(root, "BreakPad", new Vector3(center.x, 0.02f, center.z - 0.9f), new Vector3(1.4f, 0.04f, 1.0f), MaterialLib.Emissive(MaterialLib.Green, 1.2f));
            GameObject exit = MakeExit(root, "BreakExit", new Vector3(center.x, 1.0f, center.z - 0.9f), new Vector3(1.5f, 2.0f, 1.2f), ExitId.Panel, false);

            BreakablePanel bp = panel.AddComponent<BreakablePanel>();
            bp.rend = panel.GetComponent<Renderer>();
            bp.exitToEnable = exit;
            bp.hitsToBreak = 4;
            bp.minHitSpeed = 5.5f;
        }

        // --- Solucion "placa de presion": juntar peso para abrir una compuerta ----
        static PressurePlate BuildPressurePlate(Transform root)
        {
            // Placa en el fondo-izquierda.
            GameObject plate = Prim.Box(root, "PressurePlate", new Vector3(-3.4f, 0.04f, 4.4f), new Vector3(1.7f, 0.08f, 1.7f), MaterialLib.Emissive(MaterialLib.Red, 0.9f));

            // Compuerta en el piso (a la derecha) que se corre al llegar al umbral.
            GameObject cover = Prim.Box(root, "HatchCover", new Vector3(2.4f, 0.06f, 4.4f), new Vector3(1.6f, 0.12f, 1.6f), MaterialLib.Solid(MaterialLib.Metal, 0.3f, 0.5f));
            Prim.Box(root, "HatchPad", new Vector3(2.4f, 0.01f, 4.4f), new Vector3(1.4f, 0.02f, 1.4f), MaterialLib.Emissive(MaterialLib.Green, 1.2f));
            GameObject exit = MakeExit(root, "HatchExit", new Vector3(2.4f, 1.0f, 4.4f), new Vector3(1.5f, 2.0f, 1.5f), ExitId.Plate, false);

            PressurePlate pp = plate.AddComponent<PressurePlate>();
            pp.plateRend = plate.GetComponent<Renderer>();
            pp.exitToEnable = exit;
            pp.hatchCover = cover.transform;
            pp.massThreshold = 7.5f;
            return pp;
        }

        // --- Solucion "teclado": codigo de 4 digitos con pistas en la sala --------
        static KeypadController BuildKeypadAndClues(Transform root, string code, float HZ)
        {
            // Teclado a la derecha de la puerta (pared del frente).
            GameObject pad = Prim.Box(root, "Keypad", new Vector3(1.35f, 1.3f, HZ - 0.05f), new Vector3(0.4f, 0.55f, 0.1f), MaterialLib.Solid(new Color(0.12f, 0.12f, 0.14f), 0.3f, 0.6f));
            Prim.Box(pad.transform, "PadScreen", new Vector3(0f, 0.16f, -0.55f), new Vector3(0.28f, 0.14f, 0.1f), MaterialLib.Emissive(MaterialLib.Green, 1.3f), false);
            for (int r = 0; r < 3; r++)
                for (int cc = 0; cc < 3; cc++)
                    Prim.Box(pad.transform, "Btn", new Vector3(-0.09f + cc * 0.09f, -0.02f - r * 0.09f, -0.55f), new Vector3(0.06f, 0.06f, 0.08f), MaterialLib.Solid(MaterialLib.GreyLight, 0.4f), false);
            KeypadController kc = pad.AddComponent<KeypadController>();
            kc.code = code;

            // Cuatro pistas repartidas (una por digito).
            Vector3[] cluePos =
            {
                new Vector3(-2.5f, 1.7f, -5.78f),  // cerca del cuadro 1
                new Vector3(0.2f, 1.5f, -5.78f),   // cerca del cuadro 2
                new Vector3(-3.2f, 1.18f, -3.5f),  // sobre la mesa
                new Vector3(-4.33f, 2.0f, 3.2f),   // en la estanteria
            };
            for (int i = 0; i < 4; i++)
            {
                GameObject plaque = Prim.Box(root, "Clue" + (i + 1), cluePos[i], new Vector3(0.34f, 0.34f, 0.05f), MaterialLib.Emissive(new Color(0.2f, 0.7f, 0.75f), 0.7f));
                ClueObject clue = plaque.AddComponent<ClueObject>();
                clue.position = i + 1;
                clue.digit = code[i] - '0';
            }
            return kc;
        }

        // --- Backstage greybox (el reveal chocante) -----------------------------
        static void BuildBackstage(RoomBuildResult res)
        {
            GameObject bgGo = new GameObject("Backstage");
            Transform bg = bgGo.transform;
            bg.position = new Vector3(200f, 0f, 0f); // lejos, otro "mundo"
            bg.gameObject.SetActive(false);
            res.backstageRoot = bg;

            Material g1 = MaterialLib.Solid(MaterialLib.Grey, 0.05f);
            Material g2 = MaterialLib.Solid(MaterialLib.GreyLight, 0.05f);
            Material g3 = MaterialLib.Solid(MaterialLib.GreyDark, 0.05f);

            // Piso ajedrezado (lee como prototipo sin arte).
            for (int x = -6; x < 6; x++)
                for (int z = -6; z < 6; z++)
                {
                    Material m = ((x + z) % 2 == 0) ? g1 : g2;
                    Prim.Box(bg, "Tile", new Vector3(x * 2f + 1f, -0.05f, z * 2f + 1f), new Vector3(2f, 0.1f, 2f), m);
                }

            // Cajas y pilares grises sueltos, sin texturas.
            for (int i = 0; i < 10; i++)
            {
                float px = (i * 3.3f) % 20f - 10f;
                float pz = (i * 5.7f) % 18f - 9f;
                float hh = 1f + (i % 4);
                Prim.Box(bg, "GreyBlock", new Vector3(px, hh * 0.5f, pz), new Vector3(1.5f, hh, 1.5f), g3);
            }

            // Una caja "placeholder" naranja tipo dev.
            Prim.Box(bg, "DevMarker", new Vector3(0f, 1f, 8f), new Vector3(1f, 2f, 1f), MaterialLib.Emissive(MaterialLib.DevOrange, 0.8f));

            res.backstageSpawn = new Vector3(200f, 0.2f, -4f);
            res.backstageYaw = 0f;

            BackstageMarker mk = bgGo.AddComponent<BackstageMarker>();
            mk.spawn = res.backstageSpawn;
            mk.yaw = res.backstageYaw;
        }

        // --- Descubre una sala ya HORNEADA en la escena (no regenera nada) --------
        public static RoomBuildResult Discover(Room room)
        {
            RoomBuildResult res = new RoomBuildResult();
            res.room = room;
            Transform root = room.transform;

            // Objetos fisicos, alfombra y llave (re-vinculados por escaneo).
            room.grabbables.Clear();
            room.grabbables.AddRange(root.GetComponentsInChildren<Grabbable>(true));
            room.rug = root.GetComponentInChildren<RugCover>(true);
            room.key = root.GetComponentInChildren<KeyItem>(true);

            // Luces calidas de la sala (point lights).
            foreach (Light l in root.GetComponentsInChildren<Light>(true))
                if (l.type == LightType.Point) res.roomLights.Add(l);

            // Las seis salidas (cualquier IExit bajo la sala).
            foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                IExit e = mb as IExit;
                if (e != null) res.exits.Add(e);
            }

            // Backstage (root separado marcado).
            BackstageMarker mk = Object.FindFirstObjectByType<BackstageMarker>(FindObjectsInactive.Include);
            if (mk != null)
            {
                res.backstageRoot = mk.transform;
                res.backstageSpawn = mk.spawn;
                res.backstageYaw = mk.yaw;
            }
            return res;
        }
    }
}
