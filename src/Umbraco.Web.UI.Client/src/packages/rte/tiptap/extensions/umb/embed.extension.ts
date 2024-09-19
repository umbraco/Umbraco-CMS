import { UmbTiptapToolbarElementApiBase } from '../types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapEmbedExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions = () => [];

	override async execute(editor?: Editor) {
		console.log('umb-embed.execute', editor);
		// Research: https://github.com/ueberdosis/tiptap/tree/main/packages/extension-youtube
	}
}
