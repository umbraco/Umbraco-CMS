import { UmbTiptapExtensionApiBase } from '../types.js';
import { Heading } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapHeading1ExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Heading];
}
