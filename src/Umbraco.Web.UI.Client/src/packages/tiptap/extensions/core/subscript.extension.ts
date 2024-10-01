import { UmbTiptapExtensionApiBase } from '../types.js';
import { Subscript } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Subscript];
}
