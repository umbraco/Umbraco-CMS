import { UmbTiptapExtensionApiBase } from '../types.js';
import { Strike } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapStrikeExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Strike];
}
