import type { UmbPropertyEditorConfigConverter } from './property-editor-config-converter.interface.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * Converts a data type's settings when its property editor schema changes, for example when a property editor is
 * replaced by another that names its settings differently.
 *
 * When several converters are registered for the same pair of schemas, they all run on the saved settings, and a
 * setting returned by a converter of higher weight wins.
 */
export interface ManifestPropertyEditorConfigConverter extends ManifestApi<UmbPropertyEditorConfigConverter> {
	type: 'propertyEditorConfigConverter';

	/**
	 * The alias of the property editor schema the data type is moving from.
	 * @example "Umbraco.MultiNodeTreePicker"
	 */
	fromPropertyEditorSchemaAlias: string;

	/**
	 * The alias of the property editor schema the data type is moving to.
	 * @example "Umbraco.ContentPicker"
	 */
	forPropertyEditorSchemaAlias: string;
}
