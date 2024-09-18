import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { HorizontalRule } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.HorizontalRule',
	name: 'Horizontal Rule Tiptap Extension',
	api: () => import('./horizontal-rule.extension.js'),
	weight: 991,
	meta: {
		alias: 'horizontal-rule',
		icon: 'horizontal-rule',
		label: 'Horizontal Rule',
	},
};

export default class UmbTiptapHorizontalRulePlugin extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [HorizontalRule];

	override execute(editor?: Editor) {
		editor?.chain().focus().setHorizontalRule().run();
	}
}
