import { html, customElement, property, state, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import {
	PropertyEditorConfigDefaultData,
	PropertyEditorConfigProperty,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT } from '@umbraco-cms/backoffice/property-editor';

/**
 *  @element umb-property-editor-config
 *  @description - Element for displaying the configuration for a Property Editor based on a Property Editor UI Alias and a Property Editor Model alias.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbLitElement {
	/**
	 * Property Editor UI Alias. The element will render configuration for a Property Editor UI with this alias.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	private _propertyEditorUiAlias = '';
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public get propertyEditorUiAlias(): string {
		return this._propertyEditorUiAlias;
	}
	public set propertyEditorUiAlias(value: string) {
		const oldVal = this._propertyEditorUiAlias;
		this._propertyEditorUiAlias = value;
		this.requestUpdate('propertyEditorUiAlias', oldVal);
		this._observePropertyEditorUIConfig();
	}

	/**
	 * Data. The element will render configuration editors with values from this data.
	 * If a value is not found in this data, the element will use the default value from the configuration.
	 * @type {Array<{ alias: string; value: unknown }>}
	 * @attr
	 * @default []
	 */
	@property({ type: Array })
	public data: Array<{ alias: string; value: unknown }> = [];

	@state()
	private _properties: Array<PropertyEditorConfigProperty> = [];

	private _propertyEditorSchemaConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];
	private _propertyEditorUISettingsDefaultData: Array<PropertyEditorConfigDefaultData> = [];

	private _configDefaultData?: Array<PropertyEditorConfigDefaultData>;

	private _propertyEditorSchemaConfigProperties: Array<PropertyEditorConfigProperty> = [];
	private _propertyEditorUISettingsProperties: Array<PropertyEditorConfigProperty> = [];

	private _observePropertyEditorUIConfig() {
		if (!this._propertyEditorUiAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUi', this.propertyEditorUiAlias),
			(manifest) => {
				this._observePropertyEditorSchemaConfig(
					manifest?.meta.propertyEditorSchemaAlias || UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT
				);
				this._propertyEditorUISettingsProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorUISettingsDefaultData = manifest?.meta.settings?.defaultData || [];
				this._mergeConfigProperties();
				this._mergeConfigDefaultData();
			}
		);
	}

	private _observePropertyEditorSchemaConfig(propertyEditorSchemaAlias: string) {
		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorSchema', propertyEditorSchemaAlias),
			(manifest) => {
				this._propertyEditorSchemaConfigProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorSchemaConfigDefaultData = manifest?.meta.settings?.defaultData || [];
				this._mergeConfigProperties();
				this._mergeConfigDefaultData();
			}
		);
	}

	private _mergeConfigProperties() {
		this._properties = [...this._propertyEditorSchemaConfigProperties, ...this._propertyEditorUISettingsProperties];
	}

	private _mergeConfigDefaultData() {
		this._configDefaultData = [
			...this._propertyEditorSchemaConfigDefaultData,
			...this._propertyEditorUISettingsDefaultData,
		];
	}

	/**
	 * Get the stored value for a property. It will render the default value from the configuration if no value is stored in the database.
	 */
	private _getValue(property: PropertyEditorConfigProperty) {
		const value = this.data.find((data) => data.alias === property.alias)?.value;
		if (value) return value;
		const defaultValue = this._configDefaultData?.find((data) => data.alias === property.alias)?.value;
		return defaultValue ?? null;
	}

	render() {
		return html`
			${this._properties.length > 0
				? html`
						${this._properties?.map(
							(property) => html`
								<umb-workspace-property
									label="${property.label}"
									description="${ifDefined(property.description)}"
									alias="${property.alias}"
									property-editor-ui-alias="${property.propertyEditorUiAlias}"
									.value=${this._getValue(property)}
									.config=${property.config}></umb-workspace-property>
							`
						)}
				  `
				: html`<div>No configuration</div>`}
		`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-config': UmbPropertyEditorConfigElement;
	}
}
