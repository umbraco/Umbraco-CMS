import { UmbTiptapExtensionApi } from '../types.js';
import type { ManifestTiptapExtension } from '../tiptap-extension.js';
import { Code, CodeBlock } from '@umbraco-cms/backoffice/external/tiptap';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtension = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.CodeBlock',
	name: 'Code Block Tiptap Extension',
	api: () => import('./code-block.extension.js'),
	weight: 994,
	meta: {
		alias: 'code-block',
		icon: 'code-block',
		label: 'Code Block',
	},
};

export default class UmbTiptapCodeBlockExtensionApi extends UmbTiptapExtensionApi {
	getTiptapExtensions = () => [Code, CodeBlock];

	override execute(editor?: Editor) {
		// editor.chain().focus().toggleCode().run();
		editor?.chain().focus().toggleCodeBlock().run();
	}
}
