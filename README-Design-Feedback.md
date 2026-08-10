# Game Design Feedback & Direction

> This document captures design feedback, recommendations, and decisions
> based on the vision for the game.

------------------------------------------------------------------------

# Decisions Made

The following open questions have been answered. These are now the
foundation of the game.

| Question                                         | Decision                                          |
| ------------------------------------------------ | ------------------------------------------------- |
| What emotions should players feel?               | Heartwarming + concern for the other character    |
| Who is the primary audience?                     | Everyone                                          |
| What art style best supports the experience?     | Pixel art + Storybook                             |
| Should there be an overarching story?            | Yes — Scarlet and Dani are lost, trying to get home |
| How difficult should puzzles become?             | Very difficult by the end                         |
| What makes this unique?                          | The teamwork aspect                               |
| What deepens cooperation between characters?     | They are siblings                                 |
| What do coins do?                                | Spend them to unlock hints on puzzles             |

------------------------------------------------------------------------

# The Single Most Important Decision

**Scarlet and Dani are siblings.**

They are not strangers. They are not friends who met on an adventure.
They are a big sibling and a little sibling, lost together, trying to
find their way home.

The sibling relationship brings something a parent/child dynamic
cannot: equality of investment. Neither is in charge. Neither is simply
protecting the other. They need each other equally, and they both know
it.

Scarlet keeps Dani safe. Dani leads the way. Neither would make it alone.

Every puzzle, every separation, every reunion now carries weight that
no mechanic can manufacture on its own. When the player worries the
other character won't make it, they aren't worried about an AI companion.
They are worried about family.

This is the emotional core of the game. Every design decision should
protect it.

------------------------------------------------------------------------

# The Story

**Scarlet and Dani are lost.**

Something happened. They don't know the way back. Home is out there
somewhere, and the only way to reach it is together.

The story should be simple and universal:

-   No prophecy.
-   No chosen hero.
-   No villain.
-   Just family, separated from home, finding their way back.

The journey home becomes the structure of the entire game. Each world
brings them closer. Each reunion at the end of a puzzle is a small
version of the larger journey.

------------------------------------------------------------------------

# The Emotional Arc

Players should feel two things, often at the same time:

**Heartwarming**

The relationship between Scarlet and Dani should feel genuine. Small
moments — Scarlet waiting patiently, Dani leading the way, a quiet moment
between puzzles — should make players smile.

**Concern**

Players should genuinely worry. Not about dying or failing, but about
the other character. When they are separated, the player should feel the
weight of that distance. When they reunite, the relief should feel earned.

This is a difficult balance to strike, and it should inform every
design decision from music to puzzle structure to the win condition.

------------------------------------------------------------------------

# Title

**One Way Together** *(confirmed)*

It says everything the game is about in three words.

There is one way home. There is one way through every puzzle.

Together.

"Scarlet & Dani" remains a strong subtitle or tagline if a more descriptive
name is needed at launch.

------------------------------------------------------------------------

# Art Direction

**Pixel Art + Storybook**

These two styles support the emotional tone of the game differently.

**Pixel art** gives the world structure and nostalgia. It suggests
craft and care. Players associate pixel art with beloved games, and it
scales well across device sizes.

**Storybook** gives the world warmth and wonder. It suggests a fable
being told. The simple story — lost family finding their way home — fits
perfectly inside a storybook frame.

Combined, the goal is a world that feels like a classic story told
through a lovingly made game.

Visual touchstones to explore:

-   Owlboy (pixel art with emotional storytelling)
-   Tails of Iron (pixel art with illustrated narrative panels)
-   Hollow Knight (pixel world with storybook atmosphere)
-   Children's book illustration — soft colors, readable shapes

------------------------------------------------------------------------

# What Works Especially Well

## Reunite as the Win Condition

The level does not end when a character reaches an exit. It ends when
they find each other again.

Suggested level flow:

-   Characters become separated.
-   Each travels a different path.
-   Each solves different parts of the puzzle.
-   The level ends only when they reunite.

This reinforces the emotional arc at the mechanical level. Every level
is a small version of the larger story: lost, then found.

## Environmental Storytelling

Tell the story through the world:

-   Broken bridges
-   Ancient machinery
-   Abandoned camps
-   Giant statues
-   Forgotten temples
-   Faded murals
-   Nature reclaiming civilization

The player should understand the world without needing text to explain it.

## Leader / Follower Gameplay

Rather than constantly switching characters:

-   One character leads.
-   The other follows automatically.
-   Separation becomes a special gameplay moment.

This simplifies the control scheme and makes separation feel meaningful
when it happens.

------------------------------------------------------------------------

# Puzzle Design

## Signature Concept: Interlocking Solutions

Every puzzle should have two halves that require both characters.

Example:

A giant stone door blocks the path.

-   Scarlet forces it open.
-   Debris prevents Scarlet from entering.
-   Dani crawls inside.
-   Dani repairs an ancient mechanism.
-   The mechanism clears the debris.
-   Scarlet proceeds.

Neither solved the puzzle alone.

They solved each other's problems.

## Difficulty Curve

-   Early levels should be gentle and teach cooperation naturally.
-   Mid-game should introduce complexity and multi-step puzzles.
-   Late-game should be genuinely very difficult.

The player has earned a real challenge by then. Respect that.

Players who feel the game is respecting their intelligence will push
through hard puzzles. Players who feel patronized will stop.

## The Four-Step Test

Every puzzle should satisfy:

1.  Observe
2.  Understand each character's strengths
3.  Use one character to enable the other
4.  Celebrate the solution

If a puzzle can be completed by only one character, it needs another
design pass.

------------------------------------------------------------------------

# Trust as a Core Mechanic

Sometimes the characters cannot see each other.

Examples:

-   Scarlet hears machinery activate.
-   Dani sees a bridge extend.
-   Doors open somewhere else.
-   Elevators begin moving.

The player trusts the other character is making progress.

That makes every reunion more meaningful.

------------------------------------------------------------------------

# Character Design Principles

## Scarlet

-   Brave
-   Protective
-   Strong
-   Patient

Scarlet's abilities should feel powerful without feeling invincible. Scarlet
cannot go everywhere. That vulnerability is what makes Dani essential.

## Dani

-   Curious
-   Clever
-   Adventurous
-   Resourceful

Dani's abilities should feel clever without feeling fragile. Dani
cannot do everything alone. That limitation is what makes Scarlet essential.

## Equal Importance

Scarlet solves physical problems.

Dani solves exploration and precision.

Neither is more important. Neither is the "main" character.

The game should make both players (or both characters in single-player)
feel essential.

------------------------------------------------------------------------

# Hint System

Players collect coins throughout each level.

Coins are spent to unlock hints when stuck on a puzzle.

This system does several things well:

-   It rewards exploration — thorough players have more hints available.
-   It respects the difficulty curve — very hard late puzzles remain
    hard, but players have a safety valve.
-   It avoids frustration without removing the satisfaction of solving
    something on your own.
-   It creates a natural monetization path: earn coins in-game, or
    optionally purchase them.

Design considerations:

-   Hints should be layered. A first hint gives a nudge. A second hint
    gives more. A third reveals the solution. Each layer costs more coins.
-   Hide coins in places that reward curiosity — off the main path,
    behind small puzzles, in hard-to-reach spots.
-   Never put coins behind the solution to the puzzle they're in. The
    reward for exploring should be hints for the *next* puzzle, not the
    current one.

------------------------------------------------------------------------

# Recommended World Progression

Each world brings the characters closer to home and introduces one new mechanic.

| World | Theme    | New Mechanic                    |
| ----- | -------- | ------------------------------- |
| 1     | Forest   | Push / crawl basics             |
| 2     | Water    | Buoyancy, flow                  |
| 3     | Ice      | Sliding, weight                 |
| 4     | Wind     | Updrafts, balance               |
| 5     | Light    | Shadows, reflection             |
| 6     | Home     | Combined mastery of all worlds  |

The final world should combine mechanics from all previous worlds for
its hardest puzzles.

------------------------------------------------------------------------

# Core Theme

> Every challenge becomes easier when someone works alongside you.

Everything — story, puzzles, music, environments, and progression —
should reinforce this message.

------------------------------------------------------------------------

# Vision Statement

The goal is not to make another puzzle game with two characters.

The goal is to create a game where family, cooperation, and trust are
experienced through gameplay.

If players remember the relationship between the characters more than
any individual puzzle, the game has achieved its purpose.

------------------------------------------------------------------------

# Next Steps

1.  Settle on a title
2.  Characters named: Scarlet (tall) and Dani (small) — siblings ✓
3.  Relationship defined: siblings ✓
4.  Establish the visual identity with reference art
5.  Design the first 10 tutorial puzzles
6.  Playtest and validate the core cooperation loop
7.  Begin the full Game Design Document
