export type * from './property-editor-ui.extension.js';
export type * from './property-editor-ui-element.interface.js';
export type * from './property-editor-config-converter.extension.js';
export type * from './property-editor-config-converter.interface.js';

import type { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from './property-editor-ui.extension.js';
import type { ManifestPropertyEditorConfigConverter } from './property-editor-config-converter.extension.js';

type UmbPropertyEditorExtensions =
	| ManifestPropertyEditorSchema
	| ManifestPropertyEditorUi
	| ManifestPropertyEditorConfigConverter;

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorExtensions: UmbPropertyEditorExtensions;
	}
}
