Please note that this is not the final release of Badnik Framework. This is alpha release and there will be improvements later. Here is the list of known bugs and unfinished features. Here's the list of things that will be added/fixed before the final release. If you have an issue or suggestion, please add them at https://discord.gg/tsCRWPAc .

High:
- [Feature] Controller remapping
- [Feature] Tutorial prompts
- [Feature] Camera controlled with touch screen
- [Feature] Mouse click input
- [Docs] Comment the code. (Planned for 1.0)

Medium:
- [Feature] Air bubbles
- [Improvement] Enemies should do some kind of effect when they bounce after being hit

Low:
- [Improvement] Restarting has freeze instead of loading screen.
- [Improvement] Not all places in main menu have working "back" button. They require scrolling. 
- [Improvement] Pipes only support 1 player at the time.
- [Bug] Respawned CPU partners can sometimes be stuck on incorrect 2D path.
- [Improvement] Collected red rings look the same as uncollected
- [Improvement] Time records don't take characters into consideration. Save file format already supports it, but all records are saved and loaded only as "Sonic".
- [Feature] Save select menu. Multiple saves are already "supported" but there's no way of selecting which save file will be used, so it's always Save 0.
- [Feature] Hub worlds
- [Bug] Boost and drift ignore each other when they end.

- [Polish] Tails has no ranks animations. Problem: However there aren't any of them in Frontiers. Idle animation is used as a placeholder.

Won't fix:
- [Bug] Collecting Chaos Emeralds as items doesn't unlock their respective Special Stages, but also it means their Special Stage will be skipped when touching giant ring and there won't be any way to unlock them through regular gameplay. Won't fix because it's not an issue that would exist without Test Stage which shouldn't be included in the final games and the games should decide on 1 way of unlocking Super Sonic - either by finding Chaos Emeralds as items or Special Stage. Pick one or the other and there won't be this issue.

Considering:
- [Feature] Additional ability for Tails to make up for his lack of dash. I'm limited to animations from Frontiers here.

Demo game issues:
- [Improvement] Textures, particles and sound effects for Egg Nero. Not a high priority because I don't think anyone will use this boss in a their own game.
- [Optimization] Stages need more optimization for mobile. Not a high priority because those stages will be removed by other people anyway. When tested without textures it ran fine so probably materials need to be simpler.


0.1.2 requirements:
	- Animations for Tails
