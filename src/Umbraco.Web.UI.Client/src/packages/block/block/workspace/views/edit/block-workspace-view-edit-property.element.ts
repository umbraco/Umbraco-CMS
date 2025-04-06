import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-block-workspace-view-edit-property')
export class UmbBlockWorkspaceViewEditPropertyElement extends UmbLitElement {
	//
	@property({ attribute: false })
	variantId?: UmbVariantId;

	@property({ attribute: false })
	type?: UmbPropertyTypeModel;

	@state()
	_dataPath?: string;

	@state()
	_writeable?: boolean;

	@state()
	_context?: any;

	constructor() {
		super();

		// TODO: Use right context here... [NL]
		this.consumeContext('property-dataset', (context) => {
			this._context = context;
		});
	}

	override willUpdate(changedProperties: Map<string, any>) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('type') || changedProperties.has('variantId') || changedProperties.has('_context')) {
			if (this.variantId && this.type && this._context) {
				const propertyVariantId = new UmbVariantId(
					this.type.variesByCulture ? this.variantId.culture : null,
					this.type.variesBySegment ? this.variantId.segment : null,
				);
				this._dataPath = `$.values[${UmbDataPathPropertyValueQuery({
					alias: this.type.alias,
					culture: propertyVariantId.culture,
					segment: propertyVariantId.segment,
				})}].value`;
				// consume ? context
				// observe property and variantId for read-only state
				this.observe(
					observeMultiple([
						this._context.readOnly.isOnForPropertyTypeAndVariantId(this.type, propertyVariantId),
						this._context.write.isOnForPropertyTypeAndVariantId(this.type, propertyVariantId),
					]),
					([readOnly, write]) => {
						this._writeable = write && !readOnly;
					},
					'observeView',
				);
			}
		}
	}

	override render() {
		if (!this._dataPath || this._writeable === undefined) return nothing;

		return html`<umb-property-type-based-property
			class="property"
			data-path=${this._dataPath}
			.property=${this.type}
			?readonly=${!this._writeable}></umb-property-type-based-property>`;
	}
}

export default UmbBlockWorkspaceViewEditPropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-property': UmbBlockWorkspaceViewEditPropertyElement;
	}
}
