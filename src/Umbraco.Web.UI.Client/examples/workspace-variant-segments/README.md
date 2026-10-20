# Workspace Variant Segments Example

Shows how an extension can add segments to the variant options of a Document Workspace, and decide per culture where each segment is available.

## Extension types included

- **Workspace Context** - Loads a set of segments and adds them as variant options of the Document Workspace

## How it works

The Workspace Context consumes `UMB_CONTENT_WORKSPACE_CONTEXT` and uses two of its features:

- **`incomingDataHook`** - A parallel hook the workspace awaits before it presents loaded data. The example registers a method on it that loads its segments, so they are ready when the workspace is shown.
- **`variantOptionsManager`** - Holds the variant options of the workspace. The example uses `replace()` to swap its previously added options for new ones whenever the segments, the variation of the Document Type or the languages change, and `remove()` to clean up when it is destroyed.

Options are only added when the Document Type varies by segment. Each option is tied to a culture, or to a `null` culture when the Document Type does not vary by culture. The extension decides which cultures get which segments:

| Segment               | `cultures`  | Available for                                |
| --------------------- | ----------- | -------------------------------------------- |
| **Returning visitor** | `undefined` | Every culture, and content that is invariant |
| **Campaign**          | `null`      | Only content that does not vary by culture   |
| **Danish visitor**    | `['da']`    | Only the `da` culture                        |

## Try it

1. Create a Document Type and enable **Vary by segments**.
2. Create a Document of that type and open the variant selector in the workspace header. **Returning visitor** and **Campaign** are listed.
3. Enable **Vary by culture** on the Document Type as well, with at least two languages, one of them `da`. **Returning visitor** is now listed under every culture, and **Danish visitor** only under `da`. **Campaign** is not listed.

With the `documents` mock data set, use **Segment Variant Document** for step 2 and **Culture And Segment Variant Document** for step 3.
