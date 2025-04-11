import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type UmbBlockElementManager from '../../block-element-manager.js';

@customElement('umb-block-workspace-view-edit-property')
export class UmbBlockWorkspaceViewEditPropertyElement extends UmbLitElement {
	//
	@property({ attribute: false })
	variantId?: UmbVariantId;

	@property({ attribute: false })
	property?: UmbPropertyTypeModel;

	@state()
	_dataPath?: string;

	@state()
	_writeable?: boolean;

	@property({ attribute: false })
	ownerContext?: UmbBlockElementManager;

	override willUpdate(changedProperties: Map<string, any>) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('type') || changedProperties.has('variantId') || changedProperties.has('ownerContext')) {
			if (this.variantId && this.property && this.ownerContext) {
				const propertyVariantId = new UmbVariantId(
					this.property.variesByCulture ? this.variantId.culture : null,
					this.property.variesBySegment ? this.variantId.segment : null,
				);
				this._dataPath = `$.values[${UmbDataPathPropertyValueQuery({
					alias: this.property.alias,
					culture: propertyVariantId.culture,
					segment: propertyVariantId.segment,
				})}].value`;

				this.observe(
					this.ownerContext.propertyWriteGuard.isPermittedForVariantAndProperty(propertyVariantId, this.property),
					(write) => {
						this._writeable = write;
					},
					'observeView',
				);
			}
		}
	}

	override render() {
		if (!this._dataPath || this._writeable === undefined) return nothing;

		return html`<umb-property-type-based-property
			data-path=${this._dataPath}
			.property=${this.property}
			?readonly=${!this._writeable}></umb-property-type-based-property>`;
	}
}

export default UmbBlockWorkspaceViewEditPropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-property': UmbBlockWorkspaceViewEditPropertyElement;
	}
}
