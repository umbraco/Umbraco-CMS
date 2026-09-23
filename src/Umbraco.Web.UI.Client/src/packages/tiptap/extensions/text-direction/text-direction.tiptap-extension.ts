import { extensions } from '../../externals.js';

export interface UmbTiptapTextDirectionOptions {
	directions: Array<'auto' | 'ltr' | 'rtl'>;
	types: Array<string>;
}

// Overrides Tiptap's bundled `TextDirection` extension (https://github.com/ueberdosis/tiptap/pull/7207)
// to register `dir` unconditionally, defaulting to `null` instead of the editor's `textDirection` option. [LK]
export const TextDirection = extensions.TextDirection.extend<UmbTiptapTextDirectionOptions>({
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
					dir: {
						default: null,
						parseHTML: (element) => {
							const dir = element.getAttribute('dir') as 'ltr' | 'rtl' | 'auto' | null;
							return dir && this.options.directions.includes(dir) ? dir : null;
						},
						renderHTML: (attributes) => {
							if (!attributes.dir) {
								return {};
							}
							return { dir: attributes.dir };
						},
					},
				},
			},
		];
	},
});
