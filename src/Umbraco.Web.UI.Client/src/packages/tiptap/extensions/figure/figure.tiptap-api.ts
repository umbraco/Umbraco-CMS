import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { Figcaption } from './figcaption.tiptap-extension.js';
import { Figure } from './figure.tiptap-extension.js';

export default class UmbTiptapFigureExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Figcaption, Figure];
}
