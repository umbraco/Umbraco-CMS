import { UMB_WORKSPACE_SPLIT_VIEW_CONTEXT } from '../workspace-split-view.context.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-variantable-property')
export class UmbVariantablePropertyElement extends UmbLitElement {
	private _property?: PropertyTypeModelBaseModel | undefined;
	@property({ type: Object, attribute: false })
	public get property(): PropertyTypeModelBaseModel | undefined {
		return this._property;
	}
	public set property(property: PropertyTypeModelBaseModel | undefined) {
		this._property = property;
		this._updatePropertyVariantId();
	}

	private _variantContext?: typeof UMB_WORKSPACE_SPLIT_VIEW_CONTEXT.TYPE;

	@state()
	private _workspaceVariantId?: UmbVariantId;

	@state()
	private _propertyVariantId?: UmbVariantId;

	constructor() {
		super();
		// TODO: Refactor: this could use the new DataSetContext:
		this.consumeContext(UMB_WORKSPACE_SPLIT_VIEW_CONTEXT, (workspaceContext) => {
			this._variantContext = workspaceContext;
			this._observeVariantContext();
		});
	}

	private _observeVariantContext() {
		if (!this._variantContext || !this.property) return;
		this.observe(this._variantContext.variantId, (variantId) => {
			this._workspaceVariantId = variantId;
			this._updatePropertyVariantId();
		});
	}

	private _updatePropertyVariantId() {
		console.log("_updatePropertyVariantId", this._workspaceVariantId && this.property)
		if (this._workspaceVariantId && this.property) {
			const newVariantId = UmbVariantId.Create({
				culture: this.property.variesByCulture ? this._workspaceVariantId.culture : null,
				segment: this.property.variesBySegment ? this._workspaceVariantId.segment : null,
			});
			if (!this._propertyVariantId || !newVariantId.equal(this._propertyVariantId)) {
				this._propertyVariantId = newVariantId;
			}
		}
	}

	render() {
		return html`<umb-property-type-based-property
			.property=${this._property}
			.propertyVariantId=${this._propertyVariantId}></umb-property-type-based-property>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-variantable-property': UmbVariantablePropertyElement;
	}
}
