import { UmbTiptapExtensionApi } from './types.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapImageExtension extends UmbTiptapExtensionApi {
	getTiptapExtensions() {
		return [UmbImage.configure({ inline: true })];
	}
}
