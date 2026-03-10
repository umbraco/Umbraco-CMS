import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Superscript } from '../../externals.js';

export default class UmbTiptapSuperscriptExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Superscript];
}
