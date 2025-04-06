import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import type UmbBlockElementManager from '../../block-element-manager';

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

	@property({ attribute: false })
	ownerContext?: UmbBlockElementManager;

	override willUpdate(changedProperties: Map<string, any>) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('type') || changedProperties.has('variantId') || changedProperties.has('ownerContext')) {
			if (this.variantId && this.type && this.ownerContext) {
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
						this.ownerContext.propertyReadonlyGuard.permittedForVariantAndProperty(propertyVariantId, this.type),
						this.ownerContext.propertyWriteGuard.permittedForVariantAndProperty(propertyVariantId, this.type),
					]),
					([readonly, write]) => {
						this._writeable = write && !readonly;
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
