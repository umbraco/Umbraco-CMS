import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Heading3',
	name: 'Heading 3 Tiptap Extension',
	api: () => import('./heading3.extension.js'),
	weight: 947,
	meta: {
		alias: 'heading3',
		icon: 'heading3',
		label: 'Heading 3',
	},
};

export default class UmbTiptapHeading1Plugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Heading];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 3 }).run();
	}
}
