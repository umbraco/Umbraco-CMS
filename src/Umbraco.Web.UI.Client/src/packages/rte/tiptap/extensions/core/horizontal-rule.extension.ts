import { UmbTiptapExtensionApiBase } from '../types.js';
import { HorizontalRule } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHorizontalRuleExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [HorizontalRule];
}
