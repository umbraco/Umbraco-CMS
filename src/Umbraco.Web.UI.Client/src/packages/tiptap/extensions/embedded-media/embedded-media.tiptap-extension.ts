import { mergeAttributes, Node } from '../../externals.js';

export interface UmbEmbeddedMediaOptions {
	inline: boolean;
}

export const umbEmbeddedMedia = Node.create<UmbEmbeddedMediaOptions>({
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
			'data-embed-constrain': { default: true },
			'data-embed-height': { default: 240 },
			'data-embed-url': { default: null },
			'data-embed-width': { default: 360 },
			markup: { default: null, parseHTML: (element) => element.innerHTML },
		};
	},

	parseHTML() {
		return [{ tag: '.umb-embed-holder', priority: 100 }];
	},

	renderHTML({ HTMLAttributes }) {
		const { markup, ...attrs } = HTMLAttributes;
		const dom = document.createElement(this.options.inline ? 'span' : 'div');
		for (const [name, value] of Object.entries(mergeAttributes({ class: 'umb-embed-holder' }, attrs))) {
			if (value != null) dom.setAttribute(name, value);
		}
		if (markup) {
			dom.append(document.createRange().createContextualFragment(markup));
		}
		return dom;
	},

	addCommands() {
		return {
			setEmbeddedMedia:
				(options) =>
				({ commands }) => {
					const attrs = {
						markup: options.markup,
						'data-embed-url': options.url,
						'data-embed-width': options.width,
						'data-embed-height': options.height,
						'data-embed-constrain': options.constrain,
					};
					return commands.insertContent({ type: this.name, attrs });
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		umbEmbeddedMedia: {
			setEmbeddedMedia: (options: {
				markup: string;
				url: string;
				width?: string;
				height?: string;
				constrain?: boolean;
			}) => ReturnType;
		};
	}
}
