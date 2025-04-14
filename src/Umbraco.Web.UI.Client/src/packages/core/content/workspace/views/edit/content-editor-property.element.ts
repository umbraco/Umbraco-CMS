import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';

@customElement('umb-content-workspace-view-edit-property')
export class UmbContentWorkspaceViewEditPropertyElement extends UmbLitElement {
	//
	@property({ attribute: false })
	variantId?: UmbVariantId;

	@property({ attribute: false })
	property?: UmbPropertyTypeModel;

	@state()
	_dataPath?: string;

	@state()
	_writeable?: boolean;

	@state()
	_context?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (context) => {
			this._context = context;
		});
	}

	override willUpdate(changedProperties: Map<string, any>) {
		super.willUpdate(changedProperties);
		if (changedProperties.has('type') || changedProperties.has('variantId') || changedProperties.has('_context')) {
			if (this.variantId && this.property && this._context) {
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
					this._context.propertyWriteGuard.isPermittedForVariantAndProperty(propertyVariantId, this.property),
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

export default UmbContentWorkspaceViewEditPropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-workspace-view-edit-property': UmbContentWorkspaceViewEditPropertyElement;
	}
}
