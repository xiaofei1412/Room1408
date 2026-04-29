# ROOM : A Psychological Horror Escape Room 🚪

![Unity 6](https://img.shields.io/badge/Unity-6-000000.svg?style=for-the-badge&logo=unity)
![C#](https://img.shields.io/badge/C%23-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![Status](https://img.shields.io/badge/Status-Completed-success.svg?style=for-the-badge)

> *"We shape our rooms; thereafter they shape us."*

**ROOM** is a first-person psychological horror and escape room game developed in **Unity 6**, heavily inspired by Stephen King's "1408". The game places players in a seemingly ordinary hotel room that gradually transforms into a physical manifestation of their repressed trauma and guilt. 

By seamlessly integrating 3D environmental exploration with highly tactile 2D UI puzzles, and driven by a dynamic "Sanity System", *ROOM* delivers an immersive, tension-filled psychological thriller experience.

---

## 👁️ Key Features

### 🧩 Tactile UI Interactions
Say goodbye to simple point-and-click. *ROOM* bridges the gap between 3D space and 2D UI with physics-simulated interactions:
* **Rotational Mapping:** Physically drag clock hands (`UI_ClockHandDrag`) or turn the TV dial with precision.
* **Pixel-Based Wiping:** Manually wipe away condensation on a bathroom mirror to reveal hidden messages, powered by a custom pixel-array clearing algorithm.
* **Physical Drawers & Doors:** Push and pull environments using axis-constrained mouse movements (`Logic_SlidingProp`).

### 🧠 Dynamic Sanity System
Your mental state is your lifeline. Incorrect puzzle inputs or prolonged exposure to anomalies will drain your sanity:
* **Audio-Visual Hallucinations:** Low sanity triggers heavy breathing, random horror stingers, and violent camera shakes.
* **Progressive Decay:** In-game notes feature a custom `ProgressiveDecay` shader. The longer you read a cursed note, the more it bleeds, distorting the text and draining your sanity.
* **The Abyss:** Letting your sanity reach zero forces a seamless transition into a surreal, endless free-fall sequence.

### 🎭 Multi-Layered Narrative & State Machine
The game world consists of a single room controlled by a robust `Logic_StateManager`. As you uncover the truth, the room shifts between **Normal**, **Decay**, and **Horror** states, swapping 3D models, lighting, and textures in real-time. Discover 3 distinct endings, including an O. Henry-style twist and a seamless "New Game+" loop.

---

## 🖼️ Gallery

*(Gameplay screenshots showcasing the tactile puzzles and atmospheric horror)*

<p align="center">
    <img src="docs/Mirror Wiping Mechanics.png" width="48%" alt="Mirror Wiping Mechanics">
    <img src="docs/Safe Keypad Interaction.png" width="48%" alt="Safe Keypad Interaction">
</p>
<p align="center">
  <img src="docs/TV Tunting Interaction.png" width="48%" alt="TV Tunting Interaction">
  <img src="docs/Tactile Clock Puzzle.png" width="48%" alt="Tactile Clock Puzzle.png">
</p>
<p align="center">
  <img src="docs/Decaying Blood Note.png" width="48%" alt="Decaying Blood Note">
  <img src="docs/The Abyss Sequence.png" width="48%" alt="The Abyss Sequence">
</p>

---

## 🛠️ Under the Hood (Architecture)

This project was built with scalability and clean architecture in mind, utilizing modern Unity 6 features:

* **`Core_Raycaster` & Tag-Based Routing:** A centralized interaction framework that parses tags (`Inspectable`, `Operable`, `Readable`) and delegates physical interactions, 3D object inspection, or 2D UI popups without tight coupling.
* **Asynchronous Scene Management:** Utilizes Unity's `SceneManager` and `Build Profiles` to handle seamless transitions between the main game loop, the Abyss sequence, and cinematic ending credits.
* **Audio-Visual Synchronization:** The cinematic ending (Ending A) uses precision Coroutines to perfectly align UI scrolling speed, text fade-outs, and a 2m36s BGM track, achieving a millisecond-accurate dramatic hold.
* **Event-Driven UI:** The Inventory system (`UI_InventoryBar`) and Sanity HUD (`UI_SanityHearts`) are fully decoupled from the core logic, updating purely via C# `Action` delegates and UnityEvents.

---

## 🎮 Controls

* **W, A, S, D:** Move
* **Mouse:** Look around
* **Left Click:** Interact / Inspect
* **Right Click / ESC:** Exit inspection or close UI panels
* **Hold Left Click & Drag:** Manipulate physical objects (slide drawers, turn dials, wipe mirror)
* **1 - 0:** Select items from the inventory hotbar

---

## 🚀 How to Play

1.  Download the latest release from the [Releases](https://github.com/xiaofei1412/Room1408/releases/tag/v1.0.0/ROOM_Windows_v1.0.zip
) tab.
2.  Extract the `ROOM_Windows_v1.0.zip` file.
3.  Ensure `ROOM.exe` and the `ROOM_Data` folder are in the same directory.
4.  Double-click `ROOM.exe` to launch the game. *(Headphones are highly recommended for the best experience).*

*Disclaimer: Contains flashing lights, loud noises, and themes of psychological distress.*

---
*Built with Unity 6. All scripts and logic were written from scratch.*