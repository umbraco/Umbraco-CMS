export type * from './property-editor.extension.js';
export type * from './property-editor-ui-element.interface.js';

import type { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from './property-editor.extension.js';

type UmbPropertyEditorExtensions = ManifestPropertyEditorSchema | ManifestPropertyEditorUi;

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorExtensions: UmbPropertyEditorExtensions;
	}
}
