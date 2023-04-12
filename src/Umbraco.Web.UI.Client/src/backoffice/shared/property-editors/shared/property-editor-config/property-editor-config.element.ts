import { html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import type {
	PropertyEditorConfigDefaultData,
	PropertyEditorConfigProperty,
} from '@umbraco-cms/backoffice/extensions-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extensions-api';

import '../../../components/workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 *  @element umb-property-editor-config
 *  @description - Element for displaying the configuration for a Property Editor based on a Property Editor UI Alias and a Property Editor Model alias.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbLitElement {
	static styles = [UUITextStyles];

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

	private _propertyEditorModelConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];
	private _propertyEditorUIConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];

	private _configDefaultData?: Array<PropertyEditorConfigDefaultData>;

	private _propertyEditorModelConfigProperties: Array<PropertyEditorConfigProperty> = [];
	private _propertyEditorUIConfigProperties: Array<PropertyEditorConfigProperty> = [];

	private _observePropertyEditorUIConfig() {
		if (!this._propertyEditorUiAlias) return;

		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUI', this.propertyEditorUiAlias),
			(manifest) => {
				this._observePropertyEditorModelConfig(manifest?.meta.propertyEditorModel);
				this._propertyEditorUIConfigProperties = manifest?.meta.config?.properties || [];
				this._propertyEditorUIConfigDefaultData = manifest?.meta.config?.defaultData || [];
				this._mergeConfigProperties();
				this._mergeConfigDefaultData();
			}
		);
	}

	private _observePropertyEditorModelConfig(propertyEditorAlias?: string) {
		if (!propertyEditorAlias) return;

		this.observe(umbExtensionsRegistry.getByTypeAndAlias('propertyEditorModel', propertyEditorAlias), (manifest) => {
			this._propertyEditorModelConfigProperties = manifest?.meta.config?.properties || [];
			this._propertyEditorModelConfigDefaultData = manifest?.meta.config?.defaultData || [];
			this._mergeConfigProperties();
			this._mergeConfigDefaultData();
		});
	}

	private _mergeConfigProperties() {
		this._properties = [...this._propertyEditorModelConfigProperties, ...this._propertyEditorUIConfigProperties];
	}

	private _mergeConfigDefaultData() {
		this._configDefaultData = [
			...this._propertyEditorModelConfigDefaultData,
			...this._propertyEditorUIConfigDefaultData,
		];
	}

	/**
	 * Get the stored value for a property. It will render the default value from the configuration if no value is stored in the database.
	 */
	private _getValue(property: PropertyEditorConfigProperty) {
		const value = this.data.find((data) => data.alias === property.alias)?.value;
		const defaultValue = this._configDefaultData?.find((data) => data.alias === property.alias)?.value;
		return value ?? defaultValue ?? null;
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
									property-editor-ui-alias="${property.propertyEditorUI}"
									.value=${this._getValue(property)}></umb-workspace-property>
							`
						)}
				  `
				: html`<div>No configuration</div>`}
		`;
	}
}

export default UmbPropertyEditorConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-config': UmbPropertyEditorConfigElement;
	}
}
