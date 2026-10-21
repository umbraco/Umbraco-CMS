# Workspace Variant Segments Example

Shows how an extension can add segments to the variant options of a Document Workspace, based on the Document being opened, and decide per culture where each segment is available.

## Extension types included

- **Workspace Context** - Loads segments for the opened Document and adds them as variant options of the Document Workspace

## How it works

The Workspace Context consumes `UMB_CONTENT_WORKSPACE_CONTEXT` and uses two of its features:

- **`incomingDataHook`** - A parallel hook the workspace awaits before it presents loaded data. Its methods receive a meta describing the incoming content: `entityType`, `unique` and `contentType`. The example uses `contentType` to request the segments for the Document Type of the opened Document, so they are ready when the workspace is shown.
- **`variantOptionsManager`** - Holds the variant options of the workspace. The example uses `replace()` to swap its previously added options for new ones whenever the segments, the variation of the Document Type or the languages change, and `remove()` to clean up when it is destroyed.

### Segments for specific content

Every Document gets the segments in the table below. Documents of one specific Document Type also get **Newsletter subscriber**. That Document Type is set by `EXAMPLE_NEWSLETTER_DOCUMENT_TYPE_UNIQUE`, which points to a Document Type of the `documents` mock data set. Replace it with the unique of a Document Type in your own installation.

The same approach works with `unique`, for example to ask a personalization service which segments a single Document has.

### Segments for specific cultures

Options are only added when the Document Type varies by segment. Each option is tied to a culture, or to a `null` culture when the Document Type does not vary by culture. The extension decides which cultures get which segments:

| Segment                   | `availableFor` | Available for                                |
| ------------------------- | -------------- | -------------------------------------------- |
| **Returning visitor**     | `'all'`        | Every culture, and content that is invariant |
| **Campaign**              | `'invariant'`  | Only content that does not vary by culture   |
| **Danish visitor**        | `['da']`       | Only the `da` culture                        |
| **Newsletter subscriber** | `'all'`        | Every culture, for one Document Type only    |

## Try it

1. Create a Document Type and enable **Vary by segments**.
2. Create a Document of that type and open the variant selector in the workspace header. **Returning visitor** and **Campaign** are listed.
3. Enable **Vary by culture** on the Document Type as well, with at least two languages, one of them `da`. **Returning visitor** is now listed under every culture, and **Danish visitor** only under `da`. **Campaign** is not listed.
4. Set `EXAMPLE_NEWSLETTER_DOCUMENT_TYPE_UNIQUE` to the unique of that Document Type. **Newsletter subscriber** is now listed under every culture as well, while Documents of other Document Types do not get it.

With the `documents` mock data set, use **Segment Variant Document** for step 2 and **Culture And Segment Variant Document** for steps 3 and 4. The latter already uses the Document Type the example points to.
