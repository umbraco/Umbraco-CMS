import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { TrailingNode } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapTrailingNodeExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [TrailingNode];
}
