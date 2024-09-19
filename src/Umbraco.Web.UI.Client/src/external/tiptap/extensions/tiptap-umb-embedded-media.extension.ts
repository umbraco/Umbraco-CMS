import { mergeAttributes, Node } from '@tiptap/core';

export const umbEmbeddedMedia = Node.create({
	name: 'umbEmbeddedMedia',
	group() {
		return this.options.inline ? 'inline' : 'block';
	},
	inline() {
		return this.options.inline;
	},
	atom: true,
	marks: '',
	draggable: true,
	selectable: true,

	addAttributes() {
		return {
			'data-embed-constrain': { default: false },
			'data-embed-height': { default: 240 },
			'data-embed-url': { default: null },
			'data-embed-width': { default: 360 },
			markup: { default: null },
		};
	},

	parseHTML() {
		return [{ tag: 'div', class: 'umb-embed-holder', getAttrs: (node) => ({ markup: node.innerHTML }) }];
	},

	renderHTML({ HTMLAttributes }) {
		const { markup, ...attrs } = HTMLAttributes;
		const embed = document.createRange().createContextualFragment(markup);
		return ['div', mergeAttributes({ class: 'umb-embed-holder' }, attrs), embed];
	},

	addCommands() {
		return {
			setEmbeddedMedia:
				(options) =>
				({ commands }) => {
					const attrs = { markup: options.markup, 'data-embed-url': options.url };
					return commands.insertContent({ type: this.name, attrs });
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbEmbeddedMedia: {
			setEmbeddedMedia: (options: { markup: string; url: string }) => ReturnType;
		};
	}
}
