export type * from './property-editor-ui.extension.js';
export type * from './property-editor-ui-element.interface.js';

import type { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from './property-editor-ui.extension.js';

type UmbPropertyEditorExtensions = ManifestPropertyEditorSchema | ManifestPropertyEditorUi;

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorExtensions: UmbPropertyEditorExtensions;
	}
}
