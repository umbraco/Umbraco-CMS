import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Strike } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.Strike',
	name: 'Strike Tiptap Extension',
	api: () => import('./strike.extension.js'),
	weight: 996,
	meta: {
		alias: 'strike',
		icon: 'icon-strikethrough',
		label: 'Strike',
	},
};

export default class UmbTiptapStrikeExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Strike];

	override execute(editor?: Editor) {
		editor?.chain().focus().toggleStrike().run();
	}
}
