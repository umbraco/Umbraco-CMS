import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Span } from './html-tag-span.tiptap-extension.js';

export default class UmbTiptapHtmlTagSpanExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Span];
}
