import { Node } from '@tiptap/core';

export interface FigcaptionOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Figcaption = Node.create<FigcaptionOptions>({
	name: 'figcaption',

	addOptions() {
		return {
			HTMLAttributes: {},
		};
	},

	group: 'block',

	content: 'inline*',

	selectable: false,

	draggable: false,

	parseHTML() {
		return [
			{
				tag: 'figcaption',
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, HTMLAttributes, 0];
	},
});
