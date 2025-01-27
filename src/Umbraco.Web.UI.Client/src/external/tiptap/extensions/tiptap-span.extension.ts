import { Mark, mergeAttributes } from '@tiptap/core';

export interface SpanOptions {
	/**
	 * HTML attributes to add to the element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Span = Mark.create<SpanOptions>({
	name: 'span',

	group: 'inline',

	content: 'inline',

	addOptions() {
		return { HTMLAttributes: {} };
	},

	parseHTML() {
		return [{ tag: 'span' }];
	},

	renderHTML({ HTMLAttributes }) {
		return ['span', mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
