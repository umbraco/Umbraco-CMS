import { Node, mergeAttributes } from '@tiptap/core';

export interface DivOptions {
	/**
	 * HTML attributes to add to the element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Div = Node.create<DivOptions>({
	name: 'div',

	priority: 50,

	group: 'block',

	content: 'inline*',

	addOptions() {
		return { HTMLAttributes: {} };
	},

	parseHTML() {
		return [{ tag: 'div' }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['div', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
