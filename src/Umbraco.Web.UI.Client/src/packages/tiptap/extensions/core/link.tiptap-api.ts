import { UmbTiptapExtensionApiBase } from '../base.js';
import { UmbLink } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapLinkExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions() {
		return [UmbLink.configure({ openOnClick: false })];
	}
}
