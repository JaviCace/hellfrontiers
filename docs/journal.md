# Journal

## Overview

Hell Frontiers is a pixel-art action-platformer for Android, built with Unity 6.6 and tested on a Xiaomi Poco M4 Pro (Android 13). This journal records what I did each week, why I made each decision and what I learned.

## Week 1 — Toolchain, first build and character art

**What I did**

* Set up the Unity 6.6 toolchain and got a first Development build running on my phone.
* Drew the pixel-art sprites for the whole cast of the game: the **player**, the **bosses**, the **enemies** and the **skill orbs**. I did most of this during the first week and carried on into the second.

**Why I made these choices**

* I decided to do all the character art up front because I wanted to get it out of the way. Art is the part of the project I could most easily underestimate, so finishing it early meant I would not be scrambling for sprites later and could spend the rest of the semester on programming, level building and performance.
* Drawing everything first also forced me to commit to the game's identity early. The look of each boss, tied to an emotion and a colour, is part of the game's core appeal, so settling it early helped me write the Aesthetics section of the GDD with confidence.

## Week 2 — Keystore, signed build, Tiled and lifecycle

**What I did**

* Finished the remaining character sprites that I had started in week 1.
* Started building the **entire game map in Tiled**, including its art (tiles and backgrounds), instead of building levels directly in Unity.
* Researched how to bring a Tiled map into Unity. I spent a good part of the week reading documentation, forum threads and tutorials to understand how Tiled maps (tilesets, layers and collisions) can be imported and used in a Unity project.
* Created the release keystore and produced the first signed APK.
* Wrote the MDA one-pager and locked the scope.
* Checked the app lifecycle on the device (see the matrix below).

**Why I made these choices**

* I chose Tiled because designing the full map in a dedicated editor lets me iterate on level layout and art quickly, and keeps level data separate from game code.
* Planning the whole map in week 2 helped me decide the scope of the game: how many zones and bosses I can realistically finish, which links directly to the cuts list in the MDA.

**What I learned**

* Getting content out of Tiled and into Unity is a project in itself, not just a file copy. It is better to understand the pipeline early than to discover problems when the map is finished.

## Week 2, Lab B — Lifecycle, back, accessibility and scope lock

### Lifecycle matrix (Part A)

| Test                                      | Expected                                            | Result   |
| ----------------------------------------- | --------------------------------------------------- | -------- |
| Press Home, wait 10 s, return             | Paused, panel visible, audio silent, progress saved | Achieved |
| Pull the notification shade down and up   | Paused                                              | Achieved |
| Neighbour calls you, you hang up          | Paused, game resumes only on Resume                 | Achieved |
| Screen off with the power button, back on | Paused                                              | Achieved |
| Force stop from Settings, relaunch        | Progress restored from the save                     | Achieved |

---

## Week 3 — Map, Tiled-to-Unity pipeline and profiling

**What I did**

* Kept working on the **full game map and its art in Tiled**. By this point the map covers the whole game, not just the first zone.
* Kept researching and testing how to move the Tiled map into Unity.
* Ran the profiling labs on the device (below) to capture baseline performance numbers.

**Why I made these choices**

* I put the map first because the level is the backbone of an action-platformer: bosses, enemies, puzzles and orbs all depend on it. Finishing it early gives me a stable base to test combat and performance on.

## Week 3, Lab A — Rendering budgets and profiling on device

### Part B — CPU capture of worst case

* **Worst-frame main-thread ms:** 18.27 ms (median), 53.35 ms (worst frame)
* **Tallest marker:** `PostLateUpdate.FinishFrameRendering` (15.01 ms median) → inside it, `Gfx.WaitForPresentOnGfxThread` (12.32 ms) → `Semaphore.WaitForSignal` (12.32 ms)
* **Top 3 hierarchy entries (self time):**

  1. `PostLateUpdate.FinishFrameRendering` — 15.01 ms
  2. `Gfx.WaitForPresentOnGfxThread` — 12.32 ms
  3. `Semaphore.WaitForSignal` — 12.32 ms
* **GC.Collect:** Yes, it appeared 2 times during the capture, with a duration of up to 1.26 ms

### Part C — Rendering and Memory modules

* **SetPass Calls:** 5
* **Total Reserved:** 221.8 MB
* **Textures:** 64.5 MB
* **Meshes:** 13.8 KB

### Part D — RenderScaleProbe verdict

|                  | CPU  | GPU   |
| ---------------- | ---- | ----- |
| Full scale       | 5 ms | 33 ms |
| Half scale (0.5) | 5 ms | 16 ms |

* **Verdict:** GPU-bound (GPU time decreases by 51% when reducing the scale, while CPU time remains unchanged)
* **Candidate fix:** Lower the default Render Scale / reduce transparent layers

---

## Week 4 — Scripts, enemies, bosses, orbs and release

**What I did**

* This was the week where I brought the game to life with code. I added the core scripts: most of the **base scripts** and the **state machine** come from templates that I had written for games I made during my degree at my university in Spain. I adapted them to Hell Frontiers with Claude's help.
* Built the **enemies**. I wrote the `MeleeEnemy` myself first. Once it worked, I gave it to the AI and asked it to create the remaining enemies based on how my melee enemy works, so they all share the same structure and behaviour.
* Built the first boss, **AngerBoss**. I made the boss itself and the AI helped me create the **triggers** and the **boss room**.
* Built the **orbs**. I wrote my own `BaseOrb` (not a template, but the real current version) and gave it to the AI, which created the **four orbs** of the game as subclasses on top of it.
* Hardened the release pipeline, created the signed APK and wrote the awareness pack (store-asset checklist, descriptions, privacy statement) and the README.
* Spent the last stretch of the week programming full time and fixing bugs.

**Why I made these choices**

* Reusing templates from earlier projects saved a lot of time on code that every game needs (base classes, state machine). Since they are templates, Claude kept many things that this game does not use yet. I left them in on purpose, because I will need them as the game grows (more enemies, more bosses, more orb abilities).
* Writing the first version of each system myself (`MeleeEnemy`, AngerBoss, `BaseOrb`) and then asking the AI to produce the variants meant the design stayed mine. The AI extended a pattern I had already defined instead of inventing one.

**What I learned**

* A good base class pays off immediately. Once `MeleeEnemy` and `BaseOrb` worked, producing the other enemies and orbs was fast, because they only had to differ in their specific behaviour.

## AI usage

I used AI tools (Claude) during the project, mainly in week 4. I am responsible for the code I submit and I should be able to explain it. These are the ways I used it:

* **Adapting templates.** My base scripts and state machine come from my own earlier university projects. I used Claude to adapt them to Unity and to this game, and to write the scripts specific to Hell Frontiers. Because they were templates, some code is not used yet but will be later.
* **Learning C#.** The games I am used to programming are written in C++, not C#. I used the AI to help me with C# syntax and conventions and to translate what I already know into the new language.
* **Finding errors.** It helped me understand and fix compilation errors and Unity errors.
* **Speeding up repetitive work.** I wrote `MeleeEnemy` and asked for the other enemies based on how it works. I wrote `BaseOrb` and asked for the four orbs that inherit from it. For AngerBoss, it helped me create the triggers and the boss room.
* **Docs** I used AI to for the docs to get them cleaner and better explained.
* **What I did myself.** The game design, all of the art (sprites and the Tiled map), the first version of the player and enemy, boss and orb systems, and the testing on the device.


## Final reflection

My biggest problem was **time**. I did not realise how little time I had left. I thought I had more, so I relaxed during the first weeks and spent them on art and on the map. Then the last week arrived all at once. It forced me to use much more AI than I wanted to, and to spend the whole week programming and fixing bugs.

What I would do differently:

* Start programming earlier and build a playable vertical slice before finishing all the art.
* Plan the weeks backwards from the deadline, with a clear list of what must work by each date.
* Treat the art and the map as something to finish in a fixed time box, not as something to polish until it is done.
* Use the AI as a deliberate tool from the start, instead of as an emergency fix at the end.

Good things that came out of it: the art and the map are finished, the templates gave me a solid base to build on, and I learned how to bring my C++ experience into C#.
