import { UmbTiptapExtensionApiBase } from '../types.js';
import { Blockquote } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBlockquoteExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Blockquote];
}
