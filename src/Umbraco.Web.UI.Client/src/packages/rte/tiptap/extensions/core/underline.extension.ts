import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Underline } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Underline',
	name: 'Underline Tiptap Extension',
	api: () => import('./underline.extension.js'),
	weight: 997,
	meta: {
		alias: 'underline',
		icon: 'icon-underline',
		label: 'Underline',
	},
};

export default class UmbTiptapUnderlineExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Underline];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleUnderline().run();
	}
}
