export type * from './property-editor-ui.extension.js';
export type * from './property-editor-ui-element.interface.js';
export type * from './property-editor-schema-config-converter.extension.js';
export type * from './property-editor-schema-config-converter.interface.js';

import type { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from './property-editor-ui.extension.js';
import type { ManifestPropertyEditorSchemaConfigConverter } from './property-editor-schema-config-converter.extension.js';

type UmbPropertyEditorExtensions =
	| ManifestPropertyEditorSchema
	| ManifestPropertyEditorUi
	| ManifestPropertyEditorSchemaConfigConverter;

declare global {
	interface UmbExtensionManifestMap {
		UmbPropertyEditorExtensions: UmbPropertyEditorExtensions;
	}
}
