import { UmbTiptapExtensionApiBase } from '../types.js';
import { Underline } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUnderlineExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Underline];
}
