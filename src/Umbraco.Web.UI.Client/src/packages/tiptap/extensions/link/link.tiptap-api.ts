import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbLink } from './link.tiptap-extension.js';

export default class UmbTiptapLinkExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [UmbLink.configure({ openOnClick: false })];
}
