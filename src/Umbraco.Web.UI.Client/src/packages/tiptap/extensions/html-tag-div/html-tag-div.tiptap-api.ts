import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Div } from './html-tag-div.tiptap-extension.js';

export default class UmbTiptapHtmlTagDivExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Div];
}
