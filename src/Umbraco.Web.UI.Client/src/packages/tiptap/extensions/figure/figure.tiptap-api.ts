import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Figure, Figcaption } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapFigureExtensionApi extends UmbTiptapExtensionApiBase {
	// eslint-disable-next-line @typescript-eslint/no-deprecated
	getTiptapExtensions = () => [Figcaption, Figure];
}
