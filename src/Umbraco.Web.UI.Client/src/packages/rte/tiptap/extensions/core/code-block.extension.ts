import { UmbTiptapExtensionApiBase } from '../types.js';
import { Code, CodeBlock } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapCodeBlockExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Code, CodeBlock];
}
