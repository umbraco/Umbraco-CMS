import type { UmbPropertyEditorConfig } from '../index.js';
import type { UmbPropertyEditorUiElement } from './property-editor-ui-element.interface.js';
import type { ManifestElement, ManifestBase } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestPropertyEditorUi extends ManifestElement<UmbPropertyEditorUiElement> {
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
	 * @example ["Common", "Content", "Media"]
	 */
	group: string;
	/**
	 * The alias of the property editor schema that this property editor UI is for.
	 * If not specified, the property editor UI can only be used to configure other property editors.
	 * @example ["Umbraco.TextBox", "Umbraco.TextArea", "Umbraco.Label"]
	 */
	propertyEditorSchemaAlias?: string;
	settings?: PropertyEditorSettings;
	supportsReadOnly?: boolean;
	supportsDataSource?: {
		/**
		 * Whether the property editor UI is enabled for use with data sources.
		 * @type {boolean}
		 */
		enabled: boolean;
		/**
		 * A list of allowed property editor data source kinds that can be used with this property editor UI.
		 * If not specified, any data sources can be used.
		 * @example ["pickerCollection", "pickerTree"]
		 */
		forDataSourceTypes: string[];
	};
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
// eslint-disable-next-line @typescript-eslint/naming-convention
export interface PropertyEditorSettings {
	properties: PropertyEditorSettingsProperty[];
	// default data is kept separate from the properties, to give the ability for Property Editor UI to overwrite default values for the property editor settings.
	// TODO: Deprecate defaultData in the future and rename to preset. [NL]
	defaultData?: PropertyEditorSettingsDefaultData[];
}

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface PropertyEditorSettingsProperty {
	label: string;
	description?: string;
	alias: string;
	propertyEditorUiAlias: string;
	propertyEditorDataSourceAlias?: string;
	config?: UmbPropertyEditorConfig;
	weight?: number;
	validation?: {
		mandatory: boolean;
		mandatoryMessage?: string | null;
	};
}

// eslint-disable-next-line @typescript-eslint/naming-convention
export interface PropertyEditorSettingsDefaultData {
	alias: string;
	value: unknown;
}
