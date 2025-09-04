import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Underline } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapUnderlineExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Underline];
}
