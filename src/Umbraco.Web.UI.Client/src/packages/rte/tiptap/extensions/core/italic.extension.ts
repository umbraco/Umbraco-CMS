import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Italic } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Italic',
	name: 'Italic Tiptap Extension',
	api: () => import('./italic.extension.js'),
	weight: 998,
	meta: {
		alias: 'italic',
		icon: 'icon-italic',
		label: 'Italic',
	},
};

export default class UmbTiptapItalicExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Italic];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleItalic().run();
	}
}
