import { Node, mergeAttributes } from '../../externals.js';

export interface UmbTiptapHtmlTagDivOptions {
	/**
	 * HTML attributes to add to the element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Div = Node.create<UmbTiptapHtmlTagDivOptions>({
	name: 'div',

	priority: 50,

	group: 'block',

	content: 'inline*',

	addOptions() {
		return { HTMLAttributes: {} };
	},

	parseHTML() {
		return [{ tag: this.name }];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
