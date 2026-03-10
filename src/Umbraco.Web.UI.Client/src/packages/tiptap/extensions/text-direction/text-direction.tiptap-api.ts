import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

/** @deprecated No longer required, (since it comes default with Tiptap). This will be removed in Umbraco 19. [LK] */
export default class UmbTiptapTextDirectionExtensionApi extends UmbTiptapExtensionApiBase {
	// NOTE: `TextDirection` is now bundled with Tiptap since v3.11.0. [LK]
	// https://github.com/ueberdosis/tiptap/pull/7207
	getTiptapExtensions = () => [];
}
