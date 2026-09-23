# GDD — Hell Frontiers
Javier Cáceres

  -----------------------------------------------------------------------
   Name of the Game                    Hell Frontiers
  ----------------------------------- -----------------------------------
  Genre                              Platform/Action

  Number of players                   1 player

  Platform                          Android
  -----------------------------------------------------------------------

**Index**

[**Basic description**](#Basic-description)

[**Story**](#Story)

[**Aesthetics**](#Aesthetics)

[**Core Mechanics**](#Core-mechanics)

[**Dynamics**](#Dynamics)

[**Progression Content**](#Progression-Content)

[**Performance Budget**](#Performance-Budget)

[**Risks & cuts list**](#Risks-&-cuts-list)


# **Basic description**

Hell Frontiers is a pixel-art action-platformer in which the player controls an angel who has unfortunately fallen from heaven to hell.

# **Story**

The angel—the protagonist—fell into Hell due to an accident and sustained severe injuries. Their power shattered into fragments that transformed into powerful beings embodying distinct emotions; consequently, the poor angel has lost the corresponding feelings.

To return to Heaven, the angel must reclaim their power—meaning they must defeat the bosses and gradually recover their lost emotions.

# **Aesthetics**

The game features a pixel-art style and predominantly uses a dark color palette to evoke an underworld atmosphere.

Specific colors are used depending on the boss's zone—for instance, red for wrath and blue for sadness—but these are always combined with black.

# **Core Mechanics**

The player will be able to move horizontally and jump. The player will be able to attack in the cardinal directions and change orbs during the battle and use the orb's respective ability. The player will be able to interact with different puzzles along the game.

## **Game loop**

The game will be linear, making it easier to balance and design; in other words, the order in which bosses are defeated will no longer be entirely up to the player—there will be a set sequence.

The gameplay loop consists of:

Tutorial.

Defeating bosses to regain power (1–3).

Returning to the starting area to open the path to the heavens.

The path to the heavens is a "floor is lava" challenge.

Defeating the final boss.

# **Dynamics**

## **Combat System**
Basic Attack: The player has a short-range basic attack that can be executed in four directions—two horizontal and two vertical—depending on the key pressed. It consists of a quick, semi-circular strike that deals minimal damage to enemies. This basic attack can also be performed in mid-air in the same four directions, mirroring the ground-based version; while attacking, the player falls according to gravity just as they would without attacking. The basic attack is fast.

Ranged attack (feather attack): The player launches a ranged attack in the horizontal direction they are facing; it can only be fired in these two horizontal directions. The ranged attack can also be performed in mid-air—again, in either horizontal direction—exactly as it is on the ground; while attacking, the character falls according to gravity just as they would without attacking. The ranged attack speed is very fast. The projectile self-destructs after traveling a certain distance or upon colliding with an enemy or platform.

Ice Shield: The player gains an ice shield that blocks the next enemy attack, reducing the incoming damage to zero. This shield does not expire over time; it is destroyed only upon taking damage, and it regenerates after a certain period.

## **Skill System**

**Obtaining Skills**

The player can collect skill orbs by colliding with them (they are collectible objects, as detailed in the [[Interaction System](#interaction-system)]) and add them to their collection of collected orbs. The player can equip a maximum of two orbs and activate one of them.

The player can press the **Change Active Orb** key to switch between the equipped orbs. When switching, the player loses the ability granted by the previous active orb and gains the ability granted by the newly activated orb.

The player can only change their equipped orbs when they are at a **checkpoint**.

**Damage Orb**

The player deals more damage to enemies with their basic attacks.

**Shield Orb**

Unlocks the **Ice Shield** (previously specified in the [[Combat System](#combat-system)]).

**Ranged Attack Orb**

Unlocks the player's **ranged attack** (previously specified in the [[Combat System](#combat-system)]).

**Dash Orb**

Unlocks the player's **dash** (previously specified in the [[Combat System](#combat-system)]).

**Movement Speed Orb**

Increases the player's movement speed.

**Jump Orb**

Increases the player's jump height.

**Attack Range Orb**

Increases the player's melee attack range.

**Lifesteal Orb**

When the player kills an enemy, they recover health.

## **Interaction System**

**Collectible Objects**

The player can collide with (touch with their character) a collectible object to obtain the effect provided by that object.

**Checkpoint**
The player can interact with a checkpoint (by pressing the **Interact** key) to activate it and fully restore their health to maximum.

When the player dies, they respawn at the last checkpoint they activated.

**Buttons**
The player can interact with buttons that activate when colliding with them; this changes the button's texture and sends a signal to the corresponding door. These are used primarily in the SadnessBossDoor puzzle.

# **Progression Content**

My main plan is to have the first zone in week six, with the first 2 enemies and the 1st boss. I'd also like to add the second zone with the next 2 enemies and the 2nd boss. For the final week, the plan is the rest of the enemies and the last 3 bosses, including the floor is lava gameplay.

# **Risks & cuts list**

If short on time, I'd cut for week six the second zone with the next 2 enemies and the 2nd boss.