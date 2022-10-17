import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { UmbExtensionRegistry } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes, PropertyEditorConfigDefaultData, PropertyEditorConfigProperty } from '@umbraco-cms/models';
import {
	PropertyEditorConfigRef,
	UmbPropertyEditorConfigStore,
} from '../../../../core/stores/property-editor-config/property-editor-config.store';
import { UmbObserverMixin } from '../../../../core/observer';

import '../../../components/entity-property/entity-property.element';

/**
 *  @element umb-property-editor-config
 *  @description - Element for displaying the configuration for a Property Editor and Property Editor UI.
 */
@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles];

	/**
	 * Property Editor Alias. The element will render configuration for a Property Editor with this alias.
	 * @type {string}
	 * @attr
	 * @default ''
	 */
	private _propertyEditorAlias = '';
	@property({ type: String, attribute: 'property-editor-alias' })
	public get propertyEditorAlias(): string {
		return this._propertyEditorAlias;
	}
	public set propertyEditorAlias(value: string) {
		const oldVal = this._propertyEditorAlias;
		this._propertyEditorAlias = value;
		this.requestUpdate('propertyEditorAlias', oldVal);
		this._observePropertyEditorConfig();
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

	private _propertyEditorConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];
	private _propertyEditorUIConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];

	private _configDefaultData?: Array<PropertyEditorConfigDefaultData>;

	private _propertyEditorConfigProperties: Array<PropertyEditorConfigProperty> = [];
	private _propertyEditorUIConfigProperties: Array<PropertyEditorConfigProperty> = [];

	private _propertyEditorConfigStore?: UmbPropertyEditorConfigStore;
	private _extensionRegistry?: UmbExtensionRegistry;

	constructor() {
		super();

		this.consumeContext('umbPropertyEditorConfigStore', (propertyEditorConfigStore) => {
			this._propertyEditorConfigStore = propertyEditorConfigStore;
			this._observePropertyEditorConfig();
		});

		this.consumeContext('umbExtensionRegistry', (extensionRegistry) => {
			this._extensionRegistry = extensionRegistry;
			this._observePropertyEditorUIConfig();
		});
	}

	private _observePropertyEditorConfig() {
		if (!this._propertyEditorConfigStore || !this._propertyEditorAlias) return;

		this.observe<PropertyEditorConfigRef>(
			this._propertyEditorConfigStore.getByAlias(this.propertyEditorAlias),
			(propertyEditorConfig) => {
				if (!propertyEditorConfig) return;
				this._propertyEditorConfigProperties = propertyEditorConfig?.config?.properties || [];
				this._mergeProperties();
				this._propertyEditorConfigDefaultData = propertyEditorConfig?.config?.defaultData || [];
				this._mergeDefaultData();
			}
		);
	}

	private _observePropertyEditorUIConfig() {
		if (!this._extensionRegistry || !this._propertyEditorUIAlias) return;

		this.observe<ManifestTypes>(this._extensionRegistry.getByAlias(this.propertyEditorUIAlias), (manifest) => {
			if (manifest?.type === 'propertyEditorUI') {
				this._propertyEditorUIConfigProperties = manifest?.meta.config?.properties || [];
				this._mergeProperties();
				this._propertyEditorUIConfigDefaultData = manifest?.meta.config?.defaultData || [];
				this._mergeDefaultData();
			}
		});
	}

	private _mergeProperties() {
		this._properties = [...this._propertyEditorConfigProperties, ...this._propertyEditorUIConfigProperties];
	}

	private _mergeDefaultData() {
		this._configDefaultData = [...this._propertyEditorConfigDefaultData, ...this._propertyEditorUIConfigDefaultData];
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
