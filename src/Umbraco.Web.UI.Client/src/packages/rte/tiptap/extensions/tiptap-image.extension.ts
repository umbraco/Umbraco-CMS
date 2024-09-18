import { UmbTiptapExtensionBase } from './types.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapImageExtension extends UmbTiptapExtensionBase {
	getExtensions() {
		return [UmbImage.configure({ inline: true })];
	}

	getToolbarButtons() {
		return [];
	}
}
