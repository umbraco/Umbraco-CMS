import { UmbTiptapExtensionApiBase } from '../base.js';
import { umbEmbeddedMedia } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapEmbeddedMediaExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [umbEmbeddedMedia.configure({ inline: true })];
}
