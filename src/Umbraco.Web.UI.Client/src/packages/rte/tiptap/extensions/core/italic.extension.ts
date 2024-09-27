import { UmbTiptapExtensionApiBase } from '../types.js';
import { Italic } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapItalicExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Italic];
}
