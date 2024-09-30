import { UmbTiptapExtensionApiBase } from '../types.js';
import { Superscript } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Superscript];
}
