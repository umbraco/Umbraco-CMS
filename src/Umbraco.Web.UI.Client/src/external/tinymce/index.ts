/* eslint local-rules/enforce-umbraco-external-imports: 0 */
/**
 * TinyMce is a CommonJS module, but in order to make @web/test-runner happy
 * we need to load it as a default and then manually register it in the browser
 * as a global variable, so that we can find it and use it in our tests.
 * We are also loading the default icons, so that we can use them outside of a TinyMce instance.
 */
import 'tinymce';
import 'tinymce/icons/default/icons.js';
import type { RawEditorOptions, TinyMCE } from 'tinymce';

export const tinymce = window.tinymce;
export type * from 'tinymce';

export const defaultConfig: RawEditorOptions = {
	base_url: '/umbraco/backoffice/tinymce',
};

/* Initialize TinyMCE */
export function renderEditor(userConfig?: RawEditorOptions) {
	const config = { ...defaultConfig, ...userConfig };
	return tinymce.init(config);
}

// Declare a global variable to hold the TinyMCE instance
declare global {
	interface Window {
		/**
		 * @TJS-ignore
		 */
		tinymce: TinyMCE;
	}
}
