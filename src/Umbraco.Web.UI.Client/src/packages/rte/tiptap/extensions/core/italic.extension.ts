import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Italic } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Italic',
	name: 'Italic Tiptap Extension',
	api: () => import('./italic.extension.js'),
	weight: 998,
	meta: {
		alias: 'italic',
		icon: 'italic',
		label: 'Italic',
	},
};

export default class UmbTiptapItalicPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Italic];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleItalic().run();
	}
}
