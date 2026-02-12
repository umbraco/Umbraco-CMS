import { Node } from '../../externals.js';

export interface UmbTiptapFigcaptionOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Figcaption = Node.create<UmbTiptapFigcaptionOptions>({
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
		return [{ tag: this.name }];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, HTMLAttributes, 0];
	},
});
