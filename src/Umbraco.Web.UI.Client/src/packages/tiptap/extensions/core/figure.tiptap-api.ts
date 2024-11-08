import { UmbTiptapExtensionApiBase } from '../base.js';
import { Figure, Figcaption } from '@umbraco-cms/backoffice/external/tiptap';

export default class UmbTiptapFigureExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Figcaption, Figure];
}
