import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Italic } from '../../externals.js';

export default class UmbTiptapItalicExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Italic];
}
