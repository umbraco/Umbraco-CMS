import { CharacterCount } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapWordCountExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [CharacterCount.configure()];
}
