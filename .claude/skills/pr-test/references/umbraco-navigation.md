# Navigating the Umbraco Backoffice via Playwright MCP

## Backoffice Structure

The Umbraco backoffice is a single-page application (SPA) at `/umbraco`. It uses web components (Lit/UUI framework) with shadow DOM. The accessibility tree from `browser_snapshot` is your primary way to find elements.

## Login

1. Navigate to `http://localhost:{PORT}/umbraco`
2. The login page shows email and password fields
3. Look for input fields in the snapshot — they'll have labels like "Email" and "Password"
4. Fill in credentials and click the login/sign-in button
5. After login, you'll see the dashboard with sections in the left sidebar

## Main Sections (Left Sidebar)

The backoffice has these main sections, accessible via the left sidebar:

| Section | What it contains |
|---------|-----------------|
| **Content** | Content tree with documents, publishing, variants |
| **Media** | Media library with images, files, folders |
| **Settings** | Document Types, Data Types, Templates, Languages, etc. |
| **Packages** | Installed packages, marketplace |
| **Users** | User management, groups, permissions |
| **Members** | Front-end member management |
| **Dictionary** | Translation dictionary items |

To navigate to a section:
1. `browser_snapshot` to see the current page
2. Look for the section name in the sidebar navigation
3. `browser_click` on the section link

## Content Section

### Content Tree
The content tree is on the left panel. Items can be expanded by clicking the arrow/chevron.

### Creating Content
1. Right-click a content node (or use the "..." actions menu) → "Create"
2. Select the document type
3. Fill in required fields
4. Click "Save" or "Save and Publish"

### Publishing
- **Save**: Saves draft only
- **Save and Publish**: Makes content live
- **Schedule**: Set publish/unpublish dates

## Media Section

### Media Library
Similar tree structure to Content. Folders and media items.

### Uploading Media
- Click "Create" or drag-and-drop files
- For the upload field in a document, use `browser_file_upload`

## Settings Section

### Document Types
Under Settings > Document Types:
- Create new document types with the "Create" button
- Add properties by clicking "Add property" in the design tab
- Properties have a data type (property editor) picker

### Data Types
Under Settings > Data Types:
- Create new data types or modify existing ones
- Each data type uses a property editor (e.g., "Textstring", "Upload Field", "Media Picker")
- Configure settings specific to that property editor

### Templates
Under Settings > Templates:
- Razor templates for rendering content
- Clean Starter Kit installs several templates

### Languages
Under Settings > Languages:
- Add languages for variant content
- Default is English (en-US)

## Common UI Patterns

### Modals/Dialogs
- Modals appear as overlay panels
- They have a close button (X) and action buttons at the bottom
- Look for them in the snapshot as dialog elements

### Pickers
- Property editor pickers, media pickers, content pickers
- Usually open as modals with search/browse functionality
- Selection is confirmed with a "Choose" or "Submit" button

### Toast Notifications
- Success/error messages appear as toast notifications (usually top-right)
- They auto-dismiss after a few seconds
- Check for them in the snapshot right after actions

### Actions Menu
- Three-dot menu (...) on tree items and content items
- Opens a dropdown with actions like Delete, Copy, Move, Sort

## Snapshot Tips

- **Always snapshot before clicking** — the accessibility tree tells you what elements exist and their refs
- **Shadow DOM**: Umbraco uses web components with shadow DOM. The snapshot traverses into shadow roots automatically.
- **Loading states**: After navigation or actions, wait briefly (`browser_wait_for` with a short time or specific text) before snapshotting
- **Form fields**: Look for `textbox`, `combobox`, `checkbox` roles in the snapshot
- **Buttons**: Look for `button` role with descriptive names
- **Tree items**: Look for `treeitem` role for content/media tree nodes
- **Tabs**: The workspace has tabs (Content, Info, etc.) — look for `tab` role

## URL Patterns

| Page | URL Pattern |
|------|-------------|
| Dashboard | `/umbraco` |
| Content section | `/umbraco/section/content` |
| Media section | `/umbraco/section/media` |
| Settings section | `/umbraco/section/settings` |
| Specific content | `/umbraco/section/content/workspace/document/edit/{guid}` |
| Document Types | `/umbraco/section/settings/workspace/document-type/root` |
| Data Types | `/umbraco/section/settings/workspace/data-type/root` |

You can navigate directly to sections via URL, but clicking the sidebar is more reliable because it ensures the SPA state is correct.
