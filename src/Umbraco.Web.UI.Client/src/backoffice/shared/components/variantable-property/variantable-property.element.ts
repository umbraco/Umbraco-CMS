import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbVariantId } from '../../variants/variant-id.class';
import { UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN } from '../workspace/workspace-variant/workspace-variant.context';
import type { PropertyTypeViewModelBaseModel } from '@umbraco-cms/backend-api';
import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/element';

@customElement('umb-variantable-property')
export class UmbVariantablePropertyElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	private _property?: PropertyTypeViewModelBaseModel | undefined;
	@property({ type: Object, attribute: false })
	public get property(): PropertyTypeViewModelBaseModel | undefined {
		return this._property;
	}
	public set property(property: PropertyTypeViewModelBaseModel | undefined) {
		this._property = property;
		this._updatePropertyVariantId();
	}

	private _variantContext?: typeof UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN.TYPE;

	@state()
	private _workspaceVariantId?: UmbVariantId;

	@state()
	private _propertyVariantId?: UmbVariantId;

	constructor() {
		super();
		this.consumeContext(UMB_WORKSPACE_VARIANT_CONTEXT_TOKEN, (workspaceContext) => {
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-variantable-property': UmbVariantablePropertyElement;
	}
}
