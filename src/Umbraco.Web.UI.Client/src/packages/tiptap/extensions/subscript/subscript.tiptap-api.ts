import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Subscript } from '../../externals.js';

export default class UmbTiptapSubscriptExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Subscript];
}
