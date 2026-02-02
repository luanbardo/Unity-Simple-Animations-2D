## Installation

### Option 1: Unity Package (recommended)

1. Download the `.unitypackage` from the Releases page
2. Double-click it or drag it into Unity
3. Click Import

Done.

### Option 2: Manual

Clone or download this repo and copy the folder into your Unity project.

---

# SimpleAnimations2D 

**SimpleAnimations2D** is a small, beginner‑friendly 2D animation tool for Unity.

It lets you animate sprites and UI images **without using Unity’s Animator system**.  
No Animator Controllers, no state machines, no transitions — just sprites changing frames over time.

This repo is meant to be useful even if:
- you’re new to Unity
- you don’t like writing complex animation logic
- you just want something quick that *works*

---

## What problem does this solve?

Unity’s Animator is powerful, but for simple 2D games it can feel:
- confusing
- slow to set up
- overkill for basic sprite animations

**SimpleAnimations2D** is for situations like:
- “I just want this sprite to loop.”
- “Play an attack animation once, then go back to idle.”
- “Animate a UI icon.”
- “I don’t want to open the Animator window.”

Everything here is **frame‑based animation**:  
a list of sprites played in order, at a fixed speed.

---

## What’s included?

This repo gives you **four simple building blocks**.  
You can use only the ones you need.

---

## 1️⃣ Simple Sprite Animation

This is the simplest component.

You give it:
- a SpriteRenderer
- a list of sprites (frames)
- a frame rate

And it plays them in order.

### Use this if:
- you want a looping animation (fire, water, props)
- you want a simple VFX
- you don’t need multiple animation states

### How to use

1. Add `SimpleSpriteAnimation` to a GameObject
2. Make sure it has a `SpriteRenderer`
3. Assign your sprites in the **Frames** array
4. Press Play

That’s it.

You can also control it from code:
```csharp
animation.Play();
animation.Pause();
animation.Stop();
animation.Restart();
```

It can optionally:
- loop
- start at a random frame
- stop playing when off‑screen (for performance)

---

## 2️⃣ Simple Image Animation

This is the same idea as `SimpleSpriteAnimation`, but for **UI Images**.

Instead of a SpriteRenderer, it uses:
- `UnityEngine.UI.Image`

### Use this if:
- you want animated UI icons
- loading indicators
- menu effects

### How to use

1. Add `SimpleImageAnimation` to a UI Image
2. Assign sprite frames
3. Enable **Play On Enable** or call `Restart()`

```csharp
uiAnimation.Restart();
```

No Animator needed.

---

## 3️⃣ Simple Animator Clip (animation data)

A **SimpleAnimatorClip** is just data.

It stores:
- a name (example: "Idle", "Run", "Attack")
- sprite frames
- frame rate
- loop on/off

You create these as **ScriptableObjects** so they can be reused.

### Why this exists

Instead of hard‑coding frames into every object, you:
- create a clip once
- reuse it on many characters
- tweak it without touching code

Think of it as:
> “An animation preset”

### Creating clips automatically from a spritesheet (Editor Tool)

If you’re working with spritesheets, this repo includes a **small editor tool** that can turn a sliced spritesheet into a `SimpleAnimatorClip` **in one click**.

#### What this tool does 

- Takes a spritesheet texture
- Reads all the sprites inside it
- Sorts them in the correct order
- Creates a `SimpleAnimatorClip` automatically
- Places the clip in the same folder as the spritesheet

This saves you from:
- manually dragging frames one by one
- worrying about sprite order
- creating clips from scratch every time

#### How to use it 

1. Select your spritesheet texture in the **Project window**
2. Make sure it is:
   - Texture Type: **Sprite (2D and UI)**
   - Sprite Mode: **Multiple**
   - Properly sliced in the Sprite Editor
3. Right-click the texture
4. Choose:  
   **Assets → Simple Animations 2D → Create Simple Animator Clip**

That’s it.

A new `SimpleAnimatorClip` asset will be created automatically in the same folder as the spritesheet.

The generated clip will:
- use all sprites from the spritesheet
- default to 12 FPS
- be set to loop
- be named after the spritesheet (with a `SAC_` prefix)

---

## 4️⃣ Simple Sprite Animator

This is the most powerful component in the repo.

It lets a sprite:
- have multiple animations
- switch between them by name
- play one‑shot animations (like attacks)
- automatically return to a default animation

### Use this if:
- you’re animating characters or enemies
- you have Idle / Run / Attack animations
- you want control, but not Animator complexity

---

## How the Simple Sprite Animator works

1. You choose a **default animation** (usually Idle)
2. You add more animation clips (Run, Jump, Attack, etc.)
3. You tell it what to play using simple method calls

Example:
```csharp
animator.PlayClip("Run");
```

For one‑shot animations:
```csharp
animator.PlayClipOneShot("Attack");
```

When the attack finishes, it:
- automatically goes back to the default animation

You don’t need transitions, states, or parameters.

---

## Common example (very typical use case)

Let’s say you have:
- Idle (loop)
- Run (loop)
- Attack (one‑shot)

```csharp
animator.PlayClip("Idle");
animator.PlayClip("Run");
animator.PlayClipOneShot("Attack");
```

That’s the entire animation logic.

---

## Animation Events (optional)

You can react when animations finish.

Example:
```csharp
animator.OnAnimationFinished += clipName =>
{
    Debug.Log(clipName + " finished");
};
```

For one‑shot animations:
```csharp
animator.OnOneShotAnimationFinished += clipName =>
{
    Debug.Log("One shot done: " + clipName);
};
```

You can also detect frames during one‑shots (useful for hit frames).

---

## Performance notes (you don’t need to worry much)

- Animations can pause when off‑screen
- One‑shot animations always finish playing
- No Animator overhead
- No runtime allocations during playback

This is very lightweight.

---

## Requirements

- Unity 2021 or newer
- 2D project (SpriteRenderer / UI Image)
- No external packages

---

## License

MIT License  
Use it however you want — commercial or personal.

---

## Final note

This tool is intentionally **simple**.

If you want complex blending, timelines, or state graphs:
Unity’s Animator is better.

If you want:
> “Play this animation now. Stop it. Play another one.”

Then this repo is for you.
