# Unleashing Massive Flocks with Unity

![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black?logo=unity) [![Watch on YouTube](https://img.shields.io/badge/Watch%20on-YouTube-red?logo=youtube)](https://youtu.be/N55d8byORAs)

What if thousands of autonomous agents could align, separate, and cohere into a living swarm — all in real time, with blazing performance?

This project is a deep dive into boids simulation, brought to life in Unity using the Job System, Burst Compiler, and QuadTree spatial partitioning. 
Inspired by [Craig Reynolds's](https://en.wikipedia.org/wiki/Craig_Reynolds_(computer_graphics)) original flocking algorithm (1986), I reimagine it for today’s hardware: fast, scalable, and interactive.

Want to see it in action? Watch the one-minute demo on YouTube  
[![Boids simulation demo](https://img.youtube.com/vi/N55d8byORAs/hqdefault.jpg)](https://youtu.be/N55d8byORAs) 


## Why this project?

Flocking isn’t just birds in flight — it’s a window into emergent behavior, distributed AI, and multi-agent dynamics.

This simulation:

- Demonstrates real-time coordination among thousands of agents

- Teaches optimization using Unity’s multithreaded systems

- Is highly interactive and visually stunning to watch

This project shows how to scale that beauty — thousands of agents at 60+ FPS — with clean, maintainable Unity C#.

Whether you're a game developer, researcher, or just a curious Unity enthusiast — this project is for you.


## Features

Core Boid Behaviors:

- **Alignment:** steer toward the average heading of neighbors.

- **Cohesion:** move toward the average position of nearby agents.

- **Separation:** avoid collisions by keeping distance.

Advanced Interactions:

- **Predators:** autonomous agents causing local escape behavior.

- **Explosions:** double left-click to trigger a repulsion event.

- **Attraction:** drag boids by holding right mouse button.

- **Live tweaking:** UI sliders and JSON config file make all parameters tunable at runtime.

Dynamic visualization:

- **Color-coded velocity:** boids smoothly shift from light yellow to deep red based on their speed

Optimizations:

- **QuadTree:** for fast neighbor detection. [Quadtree](https://en.wikipedia.org/wiki/Quadtree)

- **Multithreaded logic:** based on Unity Job System and Burst Compiler. [Unity Jobs System](https://docs.unity3d.com/2022.3/Documentation/Manual/JobSystem.html) - [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html)


## Requirements

Unity 2022.3 LTS or newer


## How to interact with it:

| Action          				| Description								|
| ----------------------------- | ----------------------------------------- |
| Left-click      				| Spawn a new agent     					|
| Double left-click     		| Trigger an explosion (repulsion pulse)	|
| Hold right-click      		| Attract boids to the cursor     			|
| Drag with ALT + left-click    | Pan the camera     						|
| Mouse wheel scroll      		| Zoom in/out  						   		|
| Mouse wheel click      		| Open simulation settings menu		  		|


## Download

Want to try it yourself?  
👉 Download the [Windows 64-bit build.](https://drive.google.com/file/d/1hgkxZAR2s8-hbQkBrlE0WiMRDSP7pjRv/view?usp=sharing)


## Contributing

❤️ Enjoying it?
Leave a star on the repo!
Fork it, experiment, and tag me if you build something cool — I’d love to see it.
