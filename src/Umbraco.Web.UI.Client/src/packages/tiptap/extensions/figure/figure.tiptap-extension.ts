import { mergeAttributes, Node } from '../../externals.js';

export interface UmbTiptapFigureOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Figure = Node.create<UmbTiptapFigureOptions>({
	name: 'figure',
	group: 'block',
	content: 'block+',
	draggable: true,
	selectable: true,
	isolating: true,
	atom: true,

	addAttributes() {
		// `null` default avoids emitting an empty `figcaption=""` on freshly built figures.
		return {
			figcaption: { default: null },
		};
	},

	addOptions() {
		return {
			HTMLAttributes: {},
		};
	},

	parseHTML() {
		return [{ tag: this.name }];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
