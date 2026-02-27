# PR Test Plan Extraction

## Finding Test Steps in PR Descriptions

Umbraco PRs typically have well-structured descriptions with explicit test plans. Look for these section headers (case-insensitive):

- `## Test plan`
- `## Testing`
- `## How to test`
- `### Steps to test`
- `### Manual test`
- `### Manual testing`
- `## Steps to reproduce` (for bugfixes — the fix inverts the reproduction steps)
- `## Verification`

## Extracting Structured Test Data

### Preconditions

Look for setup requirements before the test steps. These may be explicitly stated or implied by the test steps themselves.

**Explicit preconditions** (stated in the PR body):
- "Create a document type with..."
- "Have a media folder with..."
- "Ensure you have..."
- "Prerequisites:"
- "Setup:"

**Implied preconditions** (deduced from test steps):
- Test says "Go to a Rich Text Editor data type" → needs a Rich Text Editor data type to exist
- Test says "Upload an image to the media section" → needs access to media section
- Test says "Create content using the Blog template" → needs the Blog document type (Clean starter kit provides this)
- Test says "Navigate to a document with variant properties" → needs document type with variants and a language setup

**Common precondition types:**
| Precondition | What it means |
|---|---|
| Document type | Need to create a document type with specific properties |
| Content node | Need to create content of a specific type |
| Media items | Need to upload images/files to the media section |
| Languages | Need to add languages beyond the default English |
| Users/groups | Need to create users with specific permissions |
| Data types | Need specific data type configurations |
| Templates | Need templates associated with document types |

Note: The Clean Starter Kit (installed during `/pr-test`) provides blog content types, templates, content, and media out of the box. Many PRs can be tested using this existing content without additional setup.

### Action Steps

Extract checkbox items (`- [ ]` or `- [x]`) and numbered steps. Map each to a browser action:

**Navigation actions:**
- "Go to {section}" → Navigate sidebar to that section (Content, Media, Settings, etc.)
- "Open {item}" → Click on a specific tree item or list item
- "Navigate to {path}" → Follow a specific path through the UI

**Interaction actions:**
- "Click {button/link}" → Click an element
- "Create {thing}" → Use the create/add workflow
- "Edit {thing}" → Open an editor
- "Save" / "Publish" → Click save/publish buttons
- "Upload {file}" → Use file upload interaction
- "Drag {source} to {target}" → Drag and drop operation
- "Select {option}" → Choose from dropdown or picker

**Verification actions:**
- "Verify {thing} appears" → Check element exists in accessibility tree
- "Verify {thing} does NOT appear" → Check element is absent
- "Ensure no errors" → Check console for JS errors
- "Check that {text} shows" → Look for specific text content
- "Should see {element}" → Verify visual element present

### Expected Outcomes

For each test step, extract what the expected result is. These are often stated as:

- "should show..." / "should display..."
- "verify that..." / "ensure that..."
- "the {thing} appears..." / "the {thing} is visible..."
- "no errors" / "no console errors"
- "the modal closes" / "the dialog dismisses"
- Checkbox items that describe the end state

### Complexity Rating

Rate each PR's test complexity:

**LOW** — Can be tested with minimal setup:
- 1-3 test steps
- Uses existing content (or Clean starter kit content)
- No special user permissions needed
- No language/variant setup
- Simple navigation and verification

**MEDIUM** — Needs some setup:
- 4-7 test steps
- Needs to create 1-2 specific document types or data types
- May need specific media uploaded
- Involves modals or multi-step workflows

**HIGH** — Significant setup required:
- 8+ test steps
- Needs variant languages configured
- Needs specific user groups/permissions
- Involves complex multi-step state (scheduled publishing, cache timing)
- Requires multiple content items with relationships
- Tests drag-and-drop or complex interactions

## Example Extraction

Given a PR with this test plan:
```
## How to test
1. Go to Settings > Data Types
2. Create a new Data Type using "Upload Field" property editor
3. Go to a Document Type and add a property using this data type
4. Create content using this Document Type
5. Upload an image file
6. Verify the filename appears as plain text below the preview
7. Save the content
8. Verify the filename is now a clickable link
```

**Extracted data:**
- **Preconditions:** None beyond default (Clean starter kit sufficient, but need to create a document type with upload field)
- **Steps:** 8 action steps (navigate, create, upload, verify, save, verify)
- **Expected outcomes:** Filename appears as text pre-save, as link post-save
- **Complexity:** MEDIUM (needs document type creation, 8 steps, but straightforward)
