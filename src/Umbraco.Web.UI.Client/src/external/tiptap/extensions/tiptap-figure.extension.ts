import { mergeAttributes, Node } from '@tiptap/core';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export interface FigureOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
export const Figure = Node.create<FigureOptions>({
	name: 'figure',
	group: 'block',
	content: 'block+',
	draggable: true,
	selectable: true,
	isolating: true,
	atom: true,

	addAttributes() {
		return {
			figcaption: {
				default: '',
			},
		};
	},

	addOptions() {
		return {
			HTMLAttributes: {},
		};
	},

	parseHTML() {
		return [
			{
				tag: this.name,
				getAttrs: (dom) => {
					const figcaption = dom.querySelector('figcaption');
					return {
						figcaption: figcaption?.textContent || '',
					};
				},
			},
		];
	},

	renderHTML({ HTMLAttributes }) {
		return [this.name, mergeAttributes(this.options.HTMLAttributes, HTMLAttributes), 0];
	},
});
