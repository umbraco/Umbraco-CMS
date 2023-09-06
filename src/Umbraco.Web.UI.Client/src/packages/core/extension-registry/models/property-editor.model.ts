import type { UmbPropertyEditorExtensionElement } from '../interfaces/index.js';
import type { UmbPropertyEditorConfig } from '../../property-editor/index.js';
import type { ManifestElement, ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyEditorUi extends ManifestElement<UmbPropertyEditorExtensionElement> {
	type: 'propertyEditorUi';
	meta: MetaPropertyEditorUi;
}

export interface MetaPropertyEditorUi {
	label: string;
	icon: string;
	/**
	 * The group that this property editor UI belongs to, which will be used to group the property editor UIs in the property editor picker.
	 * If not specified, the property editor UI will be grouped under "Common".
	 * @default "Common"
	 * @examples [
	 *  "Common",
	 * 	"Content",
	 * 	"Media"
	 * ]
	 */
	group: string;
	/**
	 * The alias of the property editor schema that this property editor UI is for.
	 * If not specified, the property editor UI can only be used to configure other property editors.
	 * @examples [
	 * 	"Umbraco.TextBox",
	 * 	"Umbraco.TextArea",
	 * 	"Umbraco.Label",
	 * ]
	 */
	propertyEditorSchemaAlias?: string;
	settings?: PropertyEditorSettings;
	supportsReadOnly?: boolean;
}

// Model
export interface ManifestPropertyEditorSchema extends ManifestBase {
	type: 'propertyEditorSchema';
	meta: MetaPropertyEditorSchema;
}

export interface MetaPropertyEditorSchema {
	defaultPropertyEditorUiAlias: string;
	settings?: PropertyEditorSettings;
}

// Config
export interface PropertyEditorSettings {
	properties: PropertyEditorConfigProperty[];
	// default data is kept separate from the properties, to give the ability for Property Editor UI to overwrite default values for the property editor settings.
	defaultData?: PropertyEditorConfigDefaultData[];
}

export interface PropertyEditorConfigProperty {
	label: string;
	description?: string;
	alias: string;
	propertyEditorUiAlias: string;
	config?: UmbPropertyEditorConfig;
}

export interface PropertyEditorConfigDefaultData {
	alias: string;
	value: unknown;
}
