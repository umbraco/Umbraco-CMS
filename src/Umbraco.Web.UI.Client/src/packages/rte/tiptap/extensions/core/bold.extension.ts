import { UmbTiptapExtensionApiBase } from '../types.js';
import { Bold } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Bold];
}
