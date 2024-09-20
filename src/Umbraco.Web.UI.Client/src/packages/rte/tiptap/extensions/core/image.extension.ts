import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapImageExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions() {
		return [UmbImage.configure({ inline: true })];
	}
}
