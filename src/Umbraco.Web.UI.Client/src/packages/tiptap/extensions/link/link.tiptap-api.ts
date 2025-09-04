import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { UmbLink } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapLinkExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [UmbLink.configure({ openOnClick: false })];
}
