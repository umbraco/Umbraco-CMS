import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Div } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHtmlTagDivExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [Div];
}
