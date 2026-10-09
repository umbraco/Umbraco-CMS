# Multi Brand Block Views Example

Custom Block Grid views for the blocks of the "Multi Brand Clothing Shop" mock data set.

Each view renders its block as it would look on the site, instead of the default block card. The styling is editorial and gallery-like: image-led cards, bold tightly-set headings, small uppercase labels and hairline borders, using UUI colour variables so it follows the theme.

- **Hero Block** - hero image with headline, subheadline and a link button.
- **Image Block** - the image with its caption.
- **Text Block** - the rendered rich text, sanitized before it is rendered.
- **Product Teaser** - the picked product's picture, name, price and stock status, plus the block's label.
- **Article Teaser** - the picked article's hero image, title and teaser.

The two teaser views also apply when the teasers are inserted into a Rich Text Editor, as in the article text.

- **One / Two Column Layout** - a dashed frame with a label, and extra spacing around the areas.

## How it works

- Every view is registered as a `blockEditorCustomView` extension for one Element Type alias, limited to the `block-grid` editor.
- The block's own values arrive as `content`, already resolved for the culture being edited.
- The teasers only hold a reference to a document, so they request the referenced document and its media, using the current culture from the variant context.

## Run it

1. `npm run example:mock` and pick `multi-brand-block-views`.
2. The example switches the mock data set to "Multi Brand Clothing Shop" for you and hides the mock set switcher.
3. Open "Little Ones Clothing Shop" or "The Outdoor Shop" in the Content section.
