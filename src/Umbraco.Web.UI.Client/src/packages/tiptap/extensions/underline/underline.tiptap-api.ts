import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Underline } from '../../externals.js';

export default class UmbTiptapUnderlineExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Underline];
}
