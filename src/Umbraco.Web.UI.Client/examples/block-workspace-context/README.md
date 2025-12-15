# Consuming Variant Context in a Block Workspace View Example
This example demonstrates how to consume and display the contextual variant information (culture and segment) from the `UMB_VARIANT_CONTEXT` within a block workspace view extension.

## What this example includes
- **Workspace View** – A custom workspace view that reads the current variant context (culture and segment) and displays it to the user.

## How it works
The workspace view extension uses the `UMB_VARIANT_CONTEXT` context token to access the current variant's culture and segment. This allows the extension to react to changes in the variant context and display the relevant information.
This pattern is useful for extension developers who need to make their workspace views aware of the current variant, enabling context-sensitive UI and logic.
