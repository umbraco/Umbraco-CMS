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
	'li',
	'table',
	'thead',
	'tbody',
	'tfoot',
	'tr',
	'td',
	'th',
	'blockquote',
	'h1',
	'h2',
	'h3',
	'h4',
	'h5',
	'h6',
	'hr',
	'figure',
	'figcaption',
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
	'dt',
	'dd',
	'form',
	'fieldset',
]);

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
