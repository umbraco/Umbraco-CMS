import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { HorizontalRule } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.HorizontalRule',
	name: 'Horizontal Rule Tiptap Extension',
	api: () => import('./horizontal-rule.extension.js'),
	weight: 991,
	meta: {
		alias: 'horizontalRule',
		icon: 'icon-horizontal-rule',
		label: 'Horizontal Rule',
	},
};

export default class UmbTiptapHorizontalRuleExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [HorizontalRule];

	override execute(editor?: Editor) {
		editor?.chain().focus().setHorizontalRule().run();
	}
}
