import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Underline } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Underline',
	name: 'Underline Tiptap Extension',
	api: () => import('./underline.extension.js'),
	weight: 997,
	meta: {
		alias: 'underline',
		icon: 'underline',
		label: 'Underline',
	},
};

export default class UmbTiptapItalicPlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Underline];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleUnderline().run();
	}
}
