UI System for Unity created by: Loags

Overview

This project is a flexible and extensible UI management system for Unity, designed to handle complex user interface (UI) interactions such as screen transitions, hierarchy management, and screen event handling. The system is built with scalability and modularity in mind, enabling you to manage multiple screens and their relationships with ease, while offering robust customization through ScriptableObject configurations.

Features

- Event-driven Architecture: Uses an event bus (UISystemEventBus) to decouple interactions between UI components, ensuring loose coupling and scalability.
- Screen Transitions: Manages transitions between screens with support for additive and exclusive screen opening.
- Screen Hierarchy Management: Handles the relationships between screens (parent-child screens) through UIScreenNode structures.
- Screen History: Tracks the history of opened screens, enabling easy back navigation and the ability to restore previous states.
- Draggable UI Components: Supports draggable UI screens and components that can be moved around within a defined area.
- Customizable Screen Configurations: Screens can be customized using UIScreenConfig assets, enabling easy configuration in the Unity Editor.
- Pause Menu & In-Game UI Support: Special managers for handling pause menus and in-game UI elements, in addition to the main menu.

Structure of the System

The system is divided into several key components:

1. UISystemManager: The core class that manages the entire UI system. It handles event subscriptions, screen hierarchy management, transitions, and screen history tracking.
2. UIScreenConfig: A ScriptableObject asset that holds the configuration for each screen. This includes settings such as whether the screen is closable and whether it is the initial screen displayed when the system starts.
3. UIScreenNode: Represents a node in the UI screen hierarchy. Each node contains information about the screen, its parent and child relationships, and whether the screen is active.
4. UIScreenNodeManager: Manages the hierarchy of UI screens by creating nodes for each screen, and handling operations like finding, adding, or removing screens from the hierarchy.
5. UIScreenRegistrar: Responsible for registering screens with the system and ensuring that screens are placed in the correct spot in the hierarchy (either as root screens or children of other screens).
6. UIScreenTransitionManager: Handles screen transitions, such as opening and closing screens with animations or effects. Supports opening screens additively (without closing other screens) or exclusively (closing all other screens).
7. UIScreenHistoryEntry: Tracks entries in the screen history stack. This allows for back navigation and restoring previous screens when needed.
8. DraggableUIComponent: A component that allows a UI element to be draggable within a defined area. It implements Unity’s IDragHandler to handle drag events and manages resetting the position of draggable components.
9. UISystemScreenDraggable: A specialized version of UISystemScreen that adds support for draggable screens. Combines the functionalities of UISystemScreen and DraggableUIComponent.
10. UISystemEvents: Defines various events (e.g., opening, closing, registering screens) that are used to communicate between different components through the event bus.
11. UISystemEventBus: The event bus that handles the publishing and subscribing of UI events. This enables a decoupled communication system between UI components.

How to Use the UI System

1. Setting Up Screens

    1. Create a UIScreenConfig:
        - Right-click in the Project window and go to Create -> UI -> UIScreenConfig.
        - Configure the UIScreenConfig:
        - Screen ID: A unique string identifier for the screen (Will be generated automatically).
        - Can Be Closed: A boolean determining whether this screen can be closed by the user.
        - Is Initial Screen: Whether this screen is the first screen to open when the system starts.

    2. Create a Screen GameObject:
        - Create a new UI Canvas in the scene (representing the UI screen).
        - Attach a component that extends from UISystemScreen (or UISystemScreenDraggable if it is a draggable screen) to this GameObject.

    3. Assign the UIScreenConfig:
        - Assign the UIScreenConfig asset you created to the screen by attaching it to the UISystemScreen component.

2. Managing Screens

    1. Open Screens:
        - Screens can be opened in two ways:
            - Exclusive: Opens a screen and closes all other screens.
            - Additive: Opens a screen while keeping the current screens active.

        Example:
        // Open a screen exclusively
        UISystemEventBus.Publish(new UIScreenOpenEvent(screenConfig, true));

        // Open a screen additively
        UISystemEventBus.Publish(new UIScreenOpenEvent(screenConfig, false));

    2. Close Screens:
        - You can close a specific screen, close the last opened screen, or close all screens.

        Example:
        // Close a specific screen
        UISystemEventBus.Publish(new UIScreenCloseEvent(screenConfig));

        // Close the last opened screen
        UISystemEventBus.Publish(new UIScreenCloseLastEvent());

        // Close all screens
        UISystemEventBus.Publish(new UIScreenCloseAllEvent());

    3. Pause Menu Integration:
        - For in-game UIs, you can integrate a pause menu by using the UISystemManagerInGame. This manager includes additional functionality to handle the escape key and toggle the pause menu.

3. Adding Draggable Screens

    1. Draggable Screens:
        - To create draggable screens, use the UISystemScreenDraggable component. Ensure the screen has a RectTransform and attach the DraggableUIComponent to handle drag events.

    2. Configuring Drag Area:
        - The DraggableUIComponent includes an optional draggableArea property. Assign a RectTransform to this field to constrain the draggable screen within a specific area.

4. Screen Hierarchy and History

    1. Hierarchy Management:
        - The system automatically manages the hierarchy of screens through UIScreenNode objects. Screens can have parent-child relationships, and these relationships are reflected in the screen's Transform hierarchy.

    2. Screen History:
        - The system tracks the history of opened screens in a stack. You can navigate back to previously opened screens by closing the current screen, which automatically restores the previous screen in the history.

Customization

    1. Adding New Screen Types:
        - You can extend the UISystemScreen class to create custom behavior for different screen types.
        - Example: If you want a specialized in-game menu, you can create a class that extends UISystemScreen and adds new features specific to in-game behavior.

    2. Custom Screen Transitions:
        - You can extend or modify the UIScreenTransitionManager to add new types of screen transitions (e.g., sliding animations, fades, etc.).

Troubleshooting

    1. Screen Not Showing:
        - Ensure that the screen's UIScreenConfig is correctly assigned.
        - Verify that the Canvas containing the screen is active and enabled.
   
    2. Screen Not Draggable:
        - Ensure the DraggableUIComponent is attached and configured correctly.
        - Verify that the draggableArea is correctly assigned (if you want to restrict the draggable area).

Future Improvements

    - Animation Integration: Adding more detailed animations for screen transitions.
    - Dynamic Loading: Support for dynamically loading screens.
    - Improved Drag Constraints: Add more complex drag constraints, such as snapping the screen to specific positions.
    - Custom screen size: Add custom screen resize during game to modify screen sizes

This UI system offers a highly modular and scalable architecture, enabling developers to manage complex UI interactions with ease. The use of an event-driven design allows for decoupled components, making it easier to expand and maintain the system over time.
