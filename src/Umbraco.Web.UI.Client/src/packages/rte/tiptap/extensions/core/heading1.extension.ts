import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Heading1',
	name: 'Heading 1 Tiptap Extension',
	api: () => import('./heading1.extension.js'),
	weight: 949,
	meta: {
		alias: 'heading1',
		icon: 'icon-heading-1',
		label: 'Heading 1',
	},
};

export default class UmbTiptapHeading1ExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Heading];

	override isActive(editor?: Editor) {
		return editor?.isActive('heading', { level: 1 }) === true;
	}

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleHeading({ level: 1 }).run();
	}
}
