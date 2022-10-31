import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes, PropertyEditorConfigDefaultData, PropertyEditorConfigProperty } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../../../components/entity-property/entity-property.element';

/**
 *  @element umb-property-editor-config
 *  @description - Element for displaying the configuration for a Property Editor based on a Property Editor UI Alias and a Property Editor Model alias.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	/**
	 * Property Editor Model Alias. The element will render configuration for a Property Editor Model with this alias.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	private _propertyEditorModelAlias = '';
	@property({ type: String, attribute: 'property-editor-model-alias' })
	public get propertyEditorModelAlias(): string {
		return this._propertyEditorModelAlias;
	}
	public set propertyEditorModelAlias(value: string) {
		const oldVal = this._propertyEditorModelAlias;
		this._propertyEditorModelAlias = value;
		this.requestUpdate('propertyEditorModelAlias', oldVal);
		this._observePropertyEditorModelConfig();
	}

	/**
	 * Property Editor UI Alias. The element will render configuration for a Property Editor UI with this alias.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	private _propertyEditorUIAlias = '';
	@property({ type: String, attribute: 'property-editor-ui-alias' })
	public get propertyEditorUIAlias(): string {
		return this._propertyEditorUIAlias;
	}
	public set propertyEditorUIAlias(value: string) {
		const oldVal = this._propertyEditorUIAlias;
		this._propertyEditorUIAlias = value;
		this.requestUpdate('propertyEditorUIAlias', oldVal);
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

	connectedCallback(): void {
		super.connectedCallback();
		this._observePropertyEditorModelConfig();
		this._observePropertyEditorUIConfig();
	}

	private _observePropertyEditorModelConfig() {
		if (!this._propertyEditorModelAlias) return;

		this.observe<ManifestTypes>(umbExtensionsRegistry.getByAlias(this.propertyEditorModelAlias), (manifest) => {
			if (manifest?.type === 'propertyEditorModel') {
				this._propertyEditorModelConfigProperties = manifest?.meta.config?.properties || [];
				this._mergeConfigProperties();
				this._propertyEditorModelConfigDefaultData = manifest?.meta.config?.defaultData || [];
				this._mergeConfigDefaultData();
			}
		});
	}

	private _observePropertyEditorUIConfig() {
		if (!this._propertyEditorUIAlias) return;

		this.observe<ManifestTypes>(umbExtensionsRegistry.getByAlias(this.propertyEditorUIAlias), (manifest) => {
			if (manifest?.type === 'propertyEditorUI') {
				this._propertyEditorUIConfigProperties = manifest?.meta.config?.properties || [];
				this._mergeConfigProperties();
				this._propertyEditorUIConfigDefaultData = manifest?.meta.config?.defaultData || [];
				this._mergeConfigDefaultData();
			}
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

	private _getValue(property: PropertyEditorConfigProperty) {
		const value = this.data.find((data) => data.alias === property.alias)?.value;
		const defaultValue = this._configDefaultData?.find((data) => data.alias === property.alias)?.value;
		return value || defaultValue || null;
	}

	render() {
		return html`
			${this._properties.length > 0
				? html`
						${this._properties?.map(
							(property) => html`
								<umb-entity-property
									label="${property.label}"
									description="${ifDefined(property.description)}"
									alias="${property.alias}"
									property-editor-ui-alias="${property.propertyEditorUI}"
									.value=${this._getValue(property)}></umb-entity-property>
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
