/* Import TinyMCE */
import * as tiny from 'tinymce';

declare const tinymce: tiny.TinyMCE;

const defaultConfig: tiny.RawEditorOptions = {
	base_url: '/tinymce',
};

/* Initialize TinyMCE */
export function renderEditor(userConfig?: tiny.RawEditorOptions) {
	const config = { ...defaultConfig, ...userConfig };
	return tinymce.init(config);
}

export * as tinymce from 'tinymce';
