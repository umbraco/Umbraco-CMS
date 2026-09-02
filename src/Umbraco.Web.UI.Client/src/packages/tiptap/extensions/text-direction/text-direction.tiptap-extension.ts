import { extensions } from '../../externals.js';
import type { Extension } from '../../externals.js';

export interface UmbTiptapTextDirectionOptions {
	/** @deprecated No longer used internally. This will be removed in Umbraco 19. [LK] */
	directions: Array<'auto' | 'ltr' | 'rtl'>;
	types: Array<string>;
}

type UmbTiptapCoreTextDirectionOptions =
	typeof extensions.TextDirection extends Extension<infer TOptions, any> ? TOptions : never;

// Overrides Tiptap's bundled `TextDirection` extension (https://github.com/ueberdosis/tiptap/pull/7207)
// to register `dir` unconditionally, defaulting to `null` instead of the editor's `textDirection` option. [LK]
export const TextDirection = extensions.TextDirection.extend<
	UmbTiptapTextDirectionOptions & UmbTiptapCoreTextDirectionOptions
>({
	addOptions() {
		return {
			direction: undefined,
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
							const dir = element.getAttribute('dir');
							if (dir === 'ltr' || dir === 'rtl' || dir === 'auto') {
								return dir;
							}
							return null;
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
