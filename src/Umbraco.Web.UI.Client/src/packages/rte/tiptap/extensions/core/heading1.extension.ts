import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Heading1',
	name: 'Heading 1 Tiptap Extension',
	api: () => import('./heading1.extension.js'),
	weight: 949,
	meta: {
		alias: 'heading1',
		icon: 'heading1',
		label: 'Heading 1',
	},
};

export default class UmbTiptapHeading1ExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Heading];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 1 }).run();
	}
}
