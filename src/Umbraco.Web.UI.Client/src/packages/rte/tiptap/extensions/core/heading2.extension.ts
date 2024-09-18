import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Heading2',
	name: 'Heading 2 Tiptap Extension',
	api: () => import('./heading2.extension.js'),
	weight: 948,
	meta: {
		alias: 'heading2',
		icon: 'heading2',
		label: 'Heading 2',
	},
};

export default class UmbTiptapHeading2ExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Heading];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 2 }).run();
	}
}
