import { Extension } from '../../externals.js';

export interface UmbTiptapTextDirectionOptions {
	directions: Array<'auto' | 'ltr' | 'rtl'>;
	types: Array<string>;
}

export const TextDirection = Extension.create<UmbTiptapTextDirectionOptions>({
	name: 'textDirection',

	addOptions() {
		return {
			directions: ['ltr', 'rtl', 'auto'],
			types: ['heading', 'paragraph'],
		};
	},

	addGlobalAttributes() {
		return [
			{
				types: this.options.types,
				attributes: {
					textDirection: {
						parseHTML: (element) => element.dir,
						renderHTML: (attributes) =>
							this.options.directions.includes(attributes.textDirection) ? { dir: attributes.textDirection } : null,
					},
				},
			},
		];
	},

	addCommands() {
		return {
			setTextDirection:
				(direction) =>
				({ commands }) => {
					return this.options.directions.includes(direction)
						? this.options.types.every((type) => commands.updateAttributes(type, { textDirection: direction }))
						: false;
				},
			unsetTextDirection:
				() =>
				({ commands }) => {
					return this.options.types.every((type) => commands.resetAttributes(type, 'textDirection'));
				},
		};
	},
});

declare module '@tiptap/core' {
	interface Commands<ReturnType> {
		textDirection: {
			setTextDirection: (direction: 'auto' | 'ltr' | 'rtl') => ReturnType;
			unsetTextDirection: () => ReturnType;
		};
	}
}
