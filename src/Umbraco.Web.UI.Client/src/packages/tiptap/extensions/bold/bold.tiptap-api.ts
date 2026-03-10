import { Bold } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Bold];
}
