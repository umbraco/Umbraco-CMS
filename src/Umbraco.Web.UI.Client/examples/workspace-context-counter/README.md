# Workspace Context Communication Across Extensions Example

The Workspace Context serves as the central communication hub for all workspace extensions. In this example, the context manages a counter that can be manipulated and displayed by different extension types, showcasing the power of shared state management in workspace extensions.

## Extension types included

This example includes:

- **Workspace Context** - Manages shared counter state and provides communication between extensions
- **Workspace Action** - Primary "Increment" button that increases the counter value
- **Workspace Action Menu Item** - "Reset Counter" dropdown option that resets the counter to zero
- **Workspace View** - Dedicated tab that displays the current counter value
- **Workspace Footer App** - Status indicator showing the counter value in the workspace footer

## How it works

All extensions communicate through the shared Workspace Context. When you increment or reset the counter using the actions, the Workspace view and Footer app update to show the new value, demonstrating reactive state management across Workspace extensions.

This pattern shows how various Workspace extensions can communicate via a Workspace Context as their mediator.
