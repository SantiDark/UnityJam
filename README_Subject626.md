# SUBJECT 626 — prototipo de jam ("múltiples soluciones")

Prototipo jugable en primera persona: **salí de la habitación**... de las **seis** formas posibles.

## Cómo se gana: el loop del test
El giro de diseño: **escapar NO termina el juego, es una ronda del test.** Cuando usás una salida:
1. Se **registra** (contador **N/6** en el HUD).
2. Esa salida se **SELLA** para las rondas siguientes — la instalación "parchea tu exploit"
   (la pared falsa gana collider, el panel vuelve reforzado, el poster queda atornillado, la
   compuerta soldada, el teclado fuera de servicio, la cerradura cambiada). **Te obliga a buscar otra.**
3. La instalación te **provoca** ("Predecible. Otra vez, sujeto 626.") y volvés a la entrada.

Ganás de verdad **cuando encontrás las 6**. Recién ahí salta el reveal grande: el arte cambia de
golpe a greybox y te felicita: *"Bien hecho, sujeto de prueba N°626."* Esto es lo que resuelve el
problema de "la victoria llega muy rápido" y lo que **motiva a buscar formas nuevas**: el juego
mismo te lo pide, te tacha la que ya usaste, y te sube la dificultad sola.

> Para cambiar cuántas salidas hacen falta, editá `ExitInfo.Count` (hoy 6).

Todo se genera **por código** al dar Play (estilo RockBottom/Deadhold/Emberlight). Sin prefabs
ni assets importados. Unity **6000.0.42f1**, URP, Input System, uGUI.

## Cómo correr
1. Abrir `D:\Unity\Subject626` con Unity 6000.0.42f1.
2. Elegí cómo generar la escena (`Assets/_Project/Scenes/Subject626.unity`):
   - **Subject626 → Hornear sala en la escena (editable a mano)** ← *recomendado para diseñar.*
     Deja el mapa como **GameObjects reales** en la escena (paredes, props, salidas), con los
     materiales guardados como assets en `Assets/Generated/`. Lo podés mover/editar a mano y
     **el Play NO lo regenera**. Volvé a hornear si querés partir de cero (pisa la escena).
   - **Subject626 → Crear escena (procedural)** ← solo un `GameBootstrap`; el mapa se arma al Play.
     Útil para iterar el código de layout, pero no editable a mano.
3. Play. (Build: **Subject626 → Build Windows** → `Build/Subject626.exe`.)

> `GameBootstrap` detecta si ya hay una sala horneada en la escena: si está, la usa tal cual;
> si no, la genera por código. El jugador, la UI y los sistemas siempre se crean al Play.
> La entrada del jugador está en el componente `Room` (`entrancePos`/`entranceYaw`), editable en el inspector.

## Controles
- **WASD** mover · **Shift** correr · **Espacio** saltar · mouse mirar
- **Clic izq** agarrar/soltar objeto · **Clic der** lanzar · **Rueda** acercar/alejar · **R** rotar
- **E** interactuar (puerta, caja, poster)
- **F1** panel de debug (probar las 3 soluciones al instante) · **R** reintentar tras escapar

## Las seis soluciones (el tema de la jam)
1. **Poster del techo** — apilá cajas, subite y sacá el poster (solo funciona estando elevado).
2. **Pared falsa** — la pared derecha *no tiene collider*: la cruzás y saltás el hueco (~2.5 m, hace
   falta **correr**) hasta la plataforma (si caés al hueco, volvés a la entrada).
3. **Puerta con llave** *(difícil de descubrir)* — la **llave está escondida debajo de la alfombra**,
   sin ninguna pista y medio metida bajo la mesa. Hay que sospechar de la alfombra, mirarla y
   **levantarla (E)**; recién ahí aparece la llave. Después, usás la puerta.
   *(Se puede sacar la llave del todo con `RoomBuilder.KeySolutionEnabled = false`: ahí la puerta
   es siempre trampa.)*
4. **Panel rajado** *(nueva, difícil)* — hay un panel débil en la pared del fondo: rompelo
   **lanzándole objetos con fuerza** (aguanta varios golpes) y salí por el hueco.
5. **Placa de presión** *(nueva, difícil)* — juntá y apilá **peso** sobre la placa hasta el umbral;
   se abre una compuerta en el piso.
6. **Teclado con código** *(nueva, difícil)* — un código de **4 dígitos** (aleatorio por partida)
   abre el teclado al lado de la puerta. Las **4 pistas** están repartidas por la sala: examinalas
   con **E** (junto a los cuadros, sobre la mesa, en la estantería) y entrá el código.

**El troll central:** abrir la puerta **sin** la llave *reinicia la sala* y te devuelve a la entrada,
con todo en su lugar. La puerta obvia no es la salida obvia.

Las 4/5/6 son a propósito **más difíciles**: piden puntería y varios lanzamientos, mover y apilar
mucho peso, o explorar para juntar el código.

### Anti-exploit
Se corrigió el *box-surfing*: mientras sostenés un objeto **no colisiona con vos**, así que no podés
pararte encima de la caja que llevás en la mano para "volar". Apilás soltándola y parándote sobre la
que quedó apoyada (esa sí tiene gravedad y colisión).

## Arquitectura (para el programador)
`Assets/_Project/Scripts/`
- **Core/** `Game` (hub estático + estado), `GameBootstrap` (arma todo al Play), `MaterialLib`
  (materiales URP por código: paleta "decorada" vs "greybox").
- **Player/** `PlayerController` (FPS CharacterController), `PlayerInteractor` (raycast E +
  `IInteractable`), `PlayerCarry` (agarrar/apilar/lanzar por física — sigue colisionando, por eso
  se puede apilar).
- **World/** `RoomBuilder` (toda la geometría, props, salidas y el backstage greybox), `Room`
  (recuerda poses iniciales y hace el reset), `Grabbable`, `OpenableCrate` + `KeyItem`,
  `DoorController`, `CeilingPoster`, `ExitTrigger`, `PitTrigger`, `Prim` (helpers de primitivas).
- **UI/** `HUD` (mira, prompt, avisos, llave, ayuda), `RevealController` (el cambio de arte +
  cartel del sujeto 626 + qué salida usaste), `DebugPanel` (F1), `UIFactory`.
- **Editor/** `SceneBuilder` (menú de escena/build).

Puntos de extensión pensados para el equipo:
- Agregar una salida nueva = un componente que llame `Game.Reveal.Escape("MI SALIDA")`.
- Toda la disposición (medidas, posiciones de cajas, dónde está la llave, el poster, panel, placa,
  teclado y pistas) está en `RoomBuilder` con constantes claras.

Perillas de dificultad (todas en `RoomBuilder`/los componentes):
- Panel: `BreakablePanel.hitsToBreak` (4) y `minHitSpeed` (5.5).
- Placa: `PressurePlate.massThreshold` (7.5) y `halfExtents`.
- Pared falsa: ancho del hueco (mover `Platform`/`Pit` en `BuildFalseWallExit`).
- Poster: `CeilingPoster.minFeetHeight` (altura mínima para alcanzarlo).
- Teclado: el código es aleatorio por partida; F1 → "Mostrar código del teclado" para testear.
- Agarre: `PlayerCarry.followForce`, `holdDist`, `throwSpeed`.

## Para el artista
Hoy todo es primitivas + materiales planos. La gracia es el **contraste**: la sala se ve
"terminada" (cálida, con muebles, cuadros, lámpara) y el **afuera** es greybox crudo (piso
ajedrezado gris, bloques sin textura, un marcador naranja tipo dev). Reemplazá:
- **Sala:** paredes/piso/muebles/props (las cajas apilables tienen que leerse como "agarrables").
- **Afuera (backstage):** dejarlo a propósito feo/prototipo — es el chiste.
- **Cartel del reveal:** hoy es UI de texto; se puede volver diegético (pantalla, luz de neón).

## Estado honesto / lo que falta
- **Sin sonido** (mudo). Sin animaciones (personaje = cámara; objetos = primitivas físicas).
- El agarre de objetos es por velocidad (tipo mano de física): sólido para apilar, pero puede
  temblar contra paredes. Ajustable en `PlayerCarry` (`followForce`, `holdDist`).
- El "conducto" del poster y la puerta al abrirse son visuales; el escape teletransporta al
  backstage (no se camina físicamente por el agujero).
- Verificado por **compile-check offline (mono mcs)**: runtime y editor compilan sin errores.
  Falta la pasada de Play real (abrir el editor) y un build.
