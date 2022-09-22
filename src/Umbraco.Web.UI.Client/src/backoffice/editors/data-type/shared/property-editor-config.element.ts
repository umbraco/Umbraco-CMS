import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextConsumerMixin } from '../../../../core/context';
import { UmbPropertyEditorConfigStore } from '../../../../core/stores/property-editor-config/property-editor-config.store';

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

	@property({ type: Array })
	public data: Array<any> = [];

	@state()
	private _properties?: Array<any> = [];

	private _propertyEditorConfigStore?: UmbPropertyEditorConfigStore;
	private _propertyEditorConfigSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbPropertyEditorConfigStore', (propertyEditorConfigStore) => {
			this._propertyEditorConfigStore = propertyEditorConfigStore;
			this._observePropertyEditorConfig();
		});
	}

	private _observePropertyEditorConfig() {
		if (!this._propertyEditorConfigStore || !this._propertyEditorAlias) return;

		this._propertyEditorConfigSubscription?.unsubscribe();

		this._propertyEditorConfigSubscription = this._propertyEditorConfigStore
			?.getByAlias(this.propertyEditorAlias)
			.subscribe((propertyEditorConfig) => {
				this._properties = propertyEditorConfig?.properties;
			});
	}

	render() {
		return html`
			<div>Property Editor Config for alias: ${this.propertyEditorAlias}</div>

			${this._properties?.map(
				(property) => html`
					<umb-entity-property
						label="${property.label}"
						description="${property.description}"
						property-editor-ui-alias="${property.propertyEditorUI}"
						.value=${this.data.find((data) => data.alias === property.alias)?.value}></umb-entity-property>
					<hr />
				`
			)}
		`;
	}
}

export default UmbPropertyEditorConfigElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-config': UmbPropertyEditorConfigElement;
	}
}
