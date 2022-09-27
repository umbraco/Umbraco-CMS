import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../core/context';
import { UmbExtensionRegistry } from '../../../../core/extension';
import { UmbPropertyEditorConfigStore } from '../../../../core/stores/property-editor-config/property-editor-config.store';

import type { PropertyEditorConfigProperty } from '../../../../core/models';

import '../../../components/entity-property/entity-property.element';

@customElement('umb-property-editor-config')
export class UmbPropertyEditorConfigElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles];

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

	@property({ type: Array })
	public data: Array<any> = [];

	@state()
	private _properties: Array<PropertyEditorConfigProperty> = [];

	private _propertyEditorConfigDefaultData?: any;
	private _propertyEditorUIConfigDefaultData?: any;

	private _propertyEditorConfigProperties: Array<PropertyEditorConfigProperty> = [];
	private _propertyEditorUIConfigProperties: Array<PropertyEditorConfigProperty> = [];

	private _propertyEditorConfigStore?: UmbPropertyEditorConfigStore;
	private _propertyEditorConfigSubscription?: Subscription;

	private _extensionRegistry?: UmbExtensionRegistry;
	private _propertyEditorUIConfigSubscription?: Subscription;

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

		this._propertyEditorConfigSubscription?.unsubscribe();

		this._propertyEditorConfigSubscription = this._propertyEditorConfigStore
			?.getByAlias(this.propertyEditorAlias)
			.subscribe((propertyEditorConfig) => {
				if (!propertyEditorConfig) return;
				this._propertyEditorConfigProperties = propertyEditorConfig?.config?.properties || [];
				this._propertyEditorConfigDefaultData = propertyEditorConfig?.config?.defaultData || {};
				this._applyProperties();
			});
	}

	private _observePropertyEditorUIConfig() {
		if (!this._extensionRegistry || !this._propertyEditorUIAlias) return;

		this._propertyEditorUIConfigSubscription?.unsubscribe();

		this._extensionRegistry?.getByAlias(this.propertyEditorUIAlias).subscribe((manifest) => {
			if (manifest?.type === 'propertyEditorUI') {
				this._propertyEditorUIConfigProperties = manifest?.meta.config?.properties || [];
				this._propertyEditorUIConfigDefaultData = manifest?.meta.config?.defaultData || {};
				this._applyProperties();
			}
		});
	}

	private _applyProperties() {
		this._properties = [...this._propertyEditorConfigProperties, ...this._propertyEditorUIConfigProperties];
	}

	private _getValue(property: PropertyEditorConfigProperty) {
		const value = this.data.find((data) => data.alias === property.alias)?.value;
		const defaultValue =
			this._propertyEditorConfigDefaultData?.[property.alias] ||
			this._propertyEditorUIConfigDefaultData?.[property.alias];
		return value || defaultValue || null;
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._propertyEditorConfigSubscription?.unsubscribe();
		this._propertyEditorUIConfigSubscription?.unsubscribe();
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
