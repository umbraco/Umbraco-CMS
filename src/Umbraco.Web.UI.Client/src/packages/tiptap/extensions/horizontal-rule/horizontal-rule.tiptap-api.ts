import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { HorizontalRule } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHorizontalRuleExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [HorizontalRule];
}
