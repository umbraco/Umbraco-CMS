import { HorizontalRule } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapHorizontalRuleExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [HorizontalRule];
}
