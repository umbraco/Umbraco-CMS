import Link from '@tiptap/extension-link';

/** @deprecated This will be relocated in Umbraco 17 to the "@umbraco-cms/backoffice/tiptap" module. [LK] */
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

	// TODO: [LK] Look to use a NodeView to render the link
	// https://tiptap.dev/docs/editor/extensions/custom-extensions/node-views/javascript

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
			setUmbLink: (options: {
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
