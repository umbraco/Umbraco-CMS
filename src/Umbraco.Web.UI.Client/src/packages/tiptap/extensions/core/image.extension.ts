import { UmbTiptapExtensionApiBase } from '../base.js';
import { UmbImage } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapImageExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions() {
		return [UmbImage.configure({ inline: true })];
	}
}
