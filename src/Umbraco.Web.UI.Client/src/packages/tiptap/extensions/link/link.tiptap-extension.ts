import { Link } from '../../externals.js';

// TODO: [LK] Look to use a NodeView to render the link, so that we remove the `data-router-slot` attribute from the HTML value.
// https://tiptap.dev/docs/editor/extensions/custom-extensions/node-views/javascript

export const UmbLink = Link.extend({
	name: 'umbLink',

	addAttributes() {
		return {
			...this.parent?.(),
			'data-anchor': { default: null },
			title: { default: null },
			type: { default: 'external' },
		};
	},

	// TODO: [LK] Review why `addOptions()` is not typed correctly here.
	// @ts-expect-error
	addOptions() {
		return {
			...this.parent?.(),
			HTMLAttributes: {
				target: '',
				'data-router-slot': 'disabled',
			},
		};
	},

	addCommands() {
		return {
			setUmbLink: (attributes) => {
				return ({ chain }) => {
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
			unsetUmbLink: () => {
				return ({ chain }) => {
					return chain().unsetMark(this.name, { extendEmptyMarkRange: true }).setMeta('preventAutolink', true).run();
				};
			},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbLink: {
			setUmbLink: (attributes: {
				type: string;
				href: string;
				'data-anchor'?: string | null;
				target?: string | null;
				title?: string | null;
			}) => ReturnType;

			unsetUmbLink: () => ReturnType;
		};
	}
}
