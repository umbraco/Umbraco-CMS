import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Bold } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Bold];
}
