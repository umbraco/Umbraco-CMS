import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Strike } from '../../externals.js';

export default class UmbTiptapStrikeExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Strike];
}
