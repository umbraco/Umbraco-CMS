/* eslint-disable local-rules/enforce-umbraco-external-imports */

import { Node } from '@tiptap/core';

export interface UmbFigcaptionOptions {
	/**
	 * HTML attributes to add to the image element.
	 * @default {}
	 * @example { class: 'foo' }
	 */
	HTMLAttributes: Record<string, any>;
}

export const Figcaption = Node.create<UmbFigcaptionOptions>({
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
