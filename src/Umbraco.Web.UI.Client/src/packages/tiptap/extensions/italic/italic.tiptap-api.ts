import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Italic } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapItalicExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Italic];
}

