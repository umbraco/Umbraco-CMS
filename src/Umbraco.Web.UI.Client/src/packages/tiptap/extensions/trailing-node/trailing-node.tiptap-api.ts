import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TrailingNode } from './trailing-node.tiptap-extension.js';

export default class UmbTiptapTrailingNodeExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [TrailingNode];
}
