import { Link } from '../../externals.js';

export const UmbLink = Link.extend({
	name: 'umbLink',

	addAttributes() {
		return {
			// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
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
			// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
			...this.parent?.(),
			HTMLAttributes: {
				target: '',
				'data-router-slot': 'disabled',
			},
		};
	},

	addCommands() {
		return {
			// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
			ensureUmbLink: (attributes) => {
				// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-expect-error
				return ({ editor, chain }) => {
					if (editor.isActive(this.name)) {
						return true;
					}
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
			// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
			setUmbLink: (attributes) => {
				// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-expect-error
				return ({ chain }) => {
					return chain().setMark(this.name, attributes).setMeta('preventAutolink', true).run();
				};
			},
			unsetUmbLink: () => {
				// TODO: [v17] Remove the `@ts-expect-error` once Tiptap has resolved the TypeScript definitions. [LK:2025-10-01]
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-expect-error
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
