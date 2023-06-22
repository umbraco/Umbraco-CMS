/* Import TinyMCE */
import tinymce, { type RawEditorOptions } from 'tinymce';

const defaultConfig: RawEditorOptions = {
	base_url: '/tinymce',
};

/* Initialize TinyMCE */
export function renderEditor(userConfig?: RawEditorOptions) {
	const config = { ...defaultConfig, ...userConfig };
	return tinymce.init(config);
}

export * as tinymce from 'tinymce';
