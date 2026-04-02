import { TrailingNode } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapTrailingNodeExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [TrailingNode];
}
