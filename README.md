# 2D Flocking Simulation

[Boids](https://en.wikipedia.org/wiki/Boids) is an artificial life simulation originally developed by [Craig Reynolds](https://en.wikipedia.org/wiki/Craig_Reynolds_(computer_graphics)).

The aim of the simulation was to replicate the behavior of flocks of birds (or shoal of fish). Instead of controlling the interactions of an entire flock, however, the Boids simulation only specifies the behavior of each individual bird.

A noteworthy aspect is the lack of any centralized control entity, instead each individual unit in the swarm follows its own defined rules, sometimes resulting in surprising overall behavior for the group as a whole.

The Boids simulation consists of a group of objects (entity) that each have their own position, velocity, acceleration and orientation. Every entity has its own perception radius and through that it can see the other entities.

The three main rules which specify the behavior of each entity are:
- **Alignment:** every entity tries to align itself with the alignment of other nearby entities perceived.
- **Separation:** every entity tries to maintain distance between itself, and other nearby entities perceived.
- **Cohesion:** every entity tries to get nearer to other nearby entities perceived.
  
On top of these, three more rules apply:

- **Repulsion:** every entity gets moved away from a specific location (your mouse pointer in case of a double click).
- **Attraction:** every entity orbits around a specific location (your mouse pointer when you keep pressing the right button).
- **Escape:** every entity tries to escape from a predator, a different entity that wander in the same space but without a specific behavior.

Boids simulation is one of the most suitable topic that benefits from the use of **multithreading** and **concurrent programming**. 

For this reason, I have used [Unity Jobs System](https://docs.unity3d.com/2022.3/Documentation/Manual/JobSystem.html) along with the [Burst Compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/index.html) that led me to put about 15-20x more entities together in the simulation.

This simulation also leverages on the use of [Quadtree](https://en.wikipedia.org/wiki/Quadtree) to efficiently compute the neighbors of every entity. A Quadtree is a spatial index structure for efficient range querying of items bounded by 2D rectangles.


## FAQ

#### Which version of Unity should I use to run this project?

This project has been developed with Unity 2022.3.32f1.

#### Is there any configuration file?

Yes, a JSON file named flocking_starter.json and located under \Assets\Resources folder is read at project startup.
It contains every simulation setting and it's a recommended set of parameters to start with. You can change these settings at runtime through the UI panel.

#### What are the keys?

- Left mouse click: to spawn a new entity.
- Double left mouse click: to make an explosion.
- Hold right mouse button: to attract the boids and make them orbiting around cursor.
- Alt + left mouse click: to navigate the scene (pan).
- Mouse wheel: to zoom in/out the scene.
- Wheel mouse click: to show/hide the UI panel.

#### What about the performances?

On a 11th Gen Intel(R) Core(TM) i7-11700KF @ 3.60GHz equipped with NVIDIA RTX GeForce 3070ti I can run the simulation with 1200 boids and 2 predators at a stable 90 FPS.
