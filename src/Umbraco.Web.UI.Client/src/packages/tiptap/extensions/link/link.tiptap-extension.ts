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
	// ref: https://github.com/ueberdosis/tiptap/issues/6670
	// eslint-disable-next-line @typescript-eslint/ban-ts-comment
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
			ensureUmbLink: (attributes) => {
				return ({ editor, chain }) => {
					if (editor.isActive(this.name)) {
						return true;
					}
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
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
			ensureUmbLink: (attributes: {
				type: string;
				href: string;
				'data-anchor'?: string | null;
				target?: string | null;
				title?: string | null;
			}) => ReturnType;

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
