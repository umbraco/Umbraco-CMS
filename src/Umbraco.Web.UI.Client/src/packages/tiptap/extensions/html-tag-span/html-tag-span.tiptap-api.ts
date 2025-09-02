import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Span } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHtmlTagSpanExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [Span];
}
