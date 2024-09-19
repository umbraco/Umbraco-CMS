import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { ManifestTiptapExtensionButtonKind } from '../tiptap-extension.js';
import { Code, CodeBlock } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.CodeBlock',
	name: 'Code Block Tiptap Extension',
	api: () => import('./code-block.extension.js'),
	weight: 994,
	meta: {
		alias: 'codeBlock',
		icon: 'icon-code',
		label: 'Code Block',
	},
};

export default class UmbTiptapCodeBlockExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [Code, CodeBlock];

	override execute(editor?: Editor) {
		// editor.chain().focus().toggleCode().run();
		editor?.chain().focus().toggleCodeBlock().run();
	}
}
