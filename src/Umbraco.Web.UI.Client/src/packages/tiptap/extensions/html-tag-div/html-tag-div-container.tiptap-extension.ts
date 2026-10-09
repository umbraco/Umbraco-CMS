import { Node, mergeAttributes } from '../../externals.js';

export interface UmbTiptapHtmlTagDivContainerOptions {
	/**
	 * HTML attributes to add to the element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

const BLOCK_TAGS = new Set([
	'div',
	'p',
	'ul',
	'ol',
	'table',
	'blockquote',
	'h1',
	'h2',
	'h3',
	'h4',
	'h5',
	'h6',
	'hr',
	'figure',
	'pre',
	'section',
	'article',
	'aside',
	'header',
	'footer',
	'nav',
	'main',
	'address',
	'dl',
	'form',
	'fieldset',
	'umb-rte-block',
]);

/**
 * Whether `element` has at least one direct child element that is block-level.
 *
 * Only checks direct children, so tags that can only ever appear nested inside another
 * `BLOCK_TAGS` entry (e.g. `li` inside `ul`/`ol`, `tr`/`td`/`th`/`thead`/`tbody`/`tfoot` inside
 * `table`, `dt`/`dd` inside `dl`, `figcaption` inside `figure`) are omitted from `BLOCK_TAGS`
 * since their container is what would actually surface as the direct child.
 *
 * `umb-rte-block` is included since it belongs to the `block` node group (`block.tiptap-extension.ts`);
 * its inline counterpart `umb-rte-block-inline` belongs to the `inline` group, so a `<div>` wrapping
 * only that is correctly left to the inline `div` node.
 * @param {HTMLElement} element The element whose direct children are checked.
 * @returns {boolean} Returns true if at least one direct child element is block-level.
 */
function hasBlockLevelChild(element: HTMLElement): boolean {
	return Array.from(element.children).some((child) => BLOCK_TAGS.has(child.tagName.toLowerCase()));
}

/**
 * A `<div>` node that contains block-level content (e.g. paragraphs, lists, headings).
 *
 * Registered alongside the inline-only `div` node (`html-tag-div.tiptap-extension.ts`) at a higher
 * parse priority: a `<div>` with at least one block-level child element is claimed by this node, so
 * its nested block content is preserved instead of being lifted out of the div (#22654). A `<div>`
 * with no block-level children falls through to the inline `div` node.
 */
export const DivContainer = Node.create<UmbTiptapHtmlTagDivContainerOptions>({
	name: 'divContainer',

	priority: 60,

	group: 'block',

	content: 'block+',

	defining: true,

	addOptions() {
		return { HTMLAttributes: {} };
	},

	parseHTML() {
		return [
			{
				tag: 'div',
				getAttrs: (element) => (hasBlockLevelChild(element as HTMLElement) ? {} : false),
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return ['div', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
