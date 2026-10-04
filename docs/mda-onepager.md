# MDA One-Pager and GDD: Hell Frontiers

**Author:** Javier Cáceres

| | |
|---|---|
| Name of the game | Hell Frontiers |
| Genre | Platform / Action |
| Players | 1 |
| Platform | Android (Unity 6.6, IL2CPP, ARM64) |

## Index

1. [Basic description](#basic-description)
2. [Story](#story)
3. [Aesthetics](#aesthetics)
4. [Mechanics](#mechanics)
5. [Dynamics](#dynamics)
6. [Progression content](#progression-content)
7. [Scope lock](#scope-lock)
8. [Cuts list](#cuts-list)
9. [Risks](#risks)
10. [Performance budget](#performance-budget)

---

## Basic description

Hell Frontiers is a pixel-art action-platformer in which the player controls an angel who has fallen from Heaven into Hell.

## Story

The angel fell into Hell because of an accident and was badly injured. Their power shattered into fragments that became powerful beings, each embodying an emotion. As a result, the angel has lost the corresponding feelings.

To return to Heaven, the angel must reclaim their power: defeat the bosses and gradually recover the lost emotions.

## Aesthetics

The experience the game aims for is **challenge and mastery** (tight combat against bosses), **discovery** (finding orbs and solving puzzles) and a **dark, atmospheric mood** (the underworld).

- Pixel-art style with a predominantly dark palette to evoke an underworld atmosphere.
- Each boss zone has its own colour (for example red for wrath and blue for sadness), always combined with black.

## Mechanics

The concrete rules and actions the player has:

- **Movement:** move horizontally and jump.
- **Basic attack:** short-range, fast, semi-circular strike in four directions (two horizontal, two vertical). It can also be done in mid-air in the same four directions. The player keeps falling under gravity while attacking. It deals minimal damage.
- **Ranged attack (feather):** a very fast projectile in the horizontal direction the player faces, usable on the ground and in mid-air. It self-destructs after a set distance or when it hits an enemy or a platform.
- **Ice shield:** blocks the next enemy attack (damage reduced to zero). It does not expire over time, it breaks only when it absorbs a hit, and it regenerates after a set period.
- **Skill orbs:** collectible by touching them. The player can equip a maximum of **two** orbs, with one active at a time. The **Change Active Orb** key switches between them in the middle of a fight. Equipped orbs can only be changed at a **checkpoint**.

| Orb | Effect |
|---|---|
| Damage | More damage with basic attacks |
| Shield | Unlocks the ice shield |
| Ranged attack | Unlocks the feather attack |
| Dash | Unlocks the dash |
| Movement speed | Faster movement |
| Jump | Higher jump |
| Attack range | Longer melee range |
| Lifesteal | Recover health when an enemy is killed |

- **Checkpoints:** pressing **Interact** activates the checkpoint and restores full health. On death, the player respawns at the last activated checkpoint.
- **Buttons and doors:** buttons activate on contact, change their texture and signal the matching door (used mainly in the SadnessBossDoor puzzle).

## Dynamics

What emerges when the player uses those mechanics:

- **Loadout decisions:** with only two equip slots and swaps limited to checkpoints, the player has to plan which orbs to bring to each boss and then switch between them in the fight (for example melee damage versus ranged safety).
- **Risk and reward in combat:** the melee attack is strong but close range; the feather is safe but weaker. The shield lets the player take one mistake, then forces them to play carefully while it recharges.
- **Boss pattern learning:** each boss has its own attack states (for example fireball and punch-platform attacks for the anger boss, and icicle, radial and water-ball attacks for the sadness boss). The player learns the patterns, then uses movement and the loadout to counter them.
- **Pacing:** checkpoints give a safe place to recover and to rebuild the loadout before the next fight.
- **Game loop (linear, to keep balance and design manageable):**
  1. Tutorial.
  2. Defeat bosses 1 to 3 to regain power.
  3. Return to the starting area to open the path to the heavens.
  4. Climb the "floor is lava" challenge.
  5. Defeat the final boss.

## Progression content

| Stage | Content |
|---|---|
| Tutorial | Movement, melee, first enemies and the first orb |
| Zone 1 (anger, red) | Two enemy types and the first boss (AngerBoss) |
| Zone 2 (sadness, blue) | Next two enemy types, the button-and-door puzzle and the second boss (SadBoss) |
| Zone 3 | Remaining enemies and the third emotion boss |
| Heaven path | "Floor is lava" climb and the final boss |

**Current state:** the full map and the art for the player, enemies, bosses and orbs are finished. The anger and sadness bosses and zones exist and are being revisited for bug fixing and animations.

## Scope lock

The scope for this semester is locked to **one linear game with the following content**. Anything not on this list is out of scope.

- 1 player character with melee, feather and ice shield.
- The **8 orb abilities** listed in Mechanics (4 already implemented).
- **4 bosses** in total: 3 emotion bosses plus the final boss.
- 1 tutorial, 3 boss zones and the "floor is lava" climb.
- Button-and-door puzzles and checkpoints with respawn.
- Local save of progress only. No network, no accounts, no ads, no purchases.
- Android phones only, landscape, 64-bit (ARM64).

**Out of scope (will not be done this semester):** multiplayer, online features, cloud save, extra zones beyond the three, a level editor, localisation, and any monetisation.

## Cuts list

If time runs short, cut in this order (first item is cut first):

1. Polish animations for enemies and bosses (keep only the gameplay-critical ones).
2. Secondary orb abilities, starting with Lifesteal and Attack range (the four not yet implemented go first).
3. The third emotion boss and its zone. The final boss would then follow the second zone directly.
4. Extra puzzle variants (keep the SadnessBossDoor puzzle only).
5. Final art for doors and decorative map details.
6. Audio polish and a settings menu.

**Never cut (the core of the game):** movement, melee, the loadout/orb system, checkpoints, at least two bosses and the final boss, local save, and the signed release build.

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| **GPU-bound rendering** (measured: GPU 33 ms at full render scale) | Low frame rate on mid-range phones | Lower the default Render Scale or add a performance mode; reduce transparent layers (see Performance budget) |
| **Time**: most art and the map were done first, programming started late | Features unfinished at the deadline | The scope lock and cuts list above; fixed order of work; one working vertical slice before adding content |
| **Tiled-to-Unity pipeline** for the whole map | Collisions or layers break after import | Test the import early with each zone and keep the Tiled files as the source of truth |
| **Boss balance and bugs** (states, triggers, boss rooms) | Frustrating or broken fights | Re-test each boss on the device and adjust cooldowns and damage |
| **Unused template code** (base scripts and state machine from earlier projects) | Extra code that is not used yet, and possible hidden bugs | Keep only what will be used as the game grows, and review code I cannot explain |
| **Device lifecycle** (pause, calls, screen off) | Lost progress or audio playing in the background | Lifecycle matrix already passed on the device; repeat after major changes |

## Performance budget

**Target device:** Xiaomi Poco M4 Pro (Android 13). Baseline captured with the Unity Profiler on the device (Week 3, Lab A).

**Baseline measurements**

| Metric | Measured |
|---|---|
| Main thread (PlayerLoop), median | 18.27 ms |
| Main thread, worst frame | 53.35 ms |
| `Gfx.WaitForPresentOnGfxThread` | 12.32 ms median, 47.79 ms maximum |
| `GC.Collect` | 2 times during the capture, up to 1.26 ms |
| SetPass calls | 5 |
| Total reserved memory | 221.8 MB (textures 64.5 MB, meshes 13.8 KB) |
| RenderScaleProbe, full scale | CPU 5 ms / GPU 33 ms |
| RenderScaleProbe, scale 0.5 | CPU 5 ms / GPU 16 ms |

**Analysis:** the game is **GPU-bound**. Lowering the render scale to 0.5 cut GPU time by 51% (33 ms to 16 ms) while CPU time stayed at 5 ms. The CPU is mostly waiting for the GPU inside `PostLateUpdate.FinishFrameRendering`, in `Gfx.WaitForPresentOnGfxThread` and `Semaphore.WaitForSignal`. Game logic is not the bottleneck.

**Budget (targets)**

| Metric | Target | Why |
|---|---|---|
| Frame time | 16.6 ms median (60 FPS); no more than 33 ms in the worst case (30 FPS) | The median is currently 18.27 ms, so the 60 FPS target needs a GPU reduction |
| GPU time | At or below 16 ms | The half-scale test reached 16 ms |
| CPU time (game logic) | At or below 8 ms | It is 5 ms now, so there is room for more enemies and bosses |
| SetPass calls | 20 or fewer | It is 5 now and the budget leaves room for new zones |
| Total reserved memory | 250 MB or less | It is 221.8 MB now |
| GC | No allocations in gameplay loops (no `GC.Collect` during a fight) | Avoids frame spikes |
| APK size | To be measured on the release build and recorded in the README | Keep the sideloaded APK reasonable |

**Candidate fix:** lower the default Render Scale (or add a performance mode that reduces it) and reduce transparent layers. Then repeat the RenderScaleProbe and the CPU capture to confirm the median frame time reaches the target.
