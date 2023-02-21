import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbVariantId } from '../../variants/variant-id.class';
import type { PropertyTypeViewModelBaseModel } from '@umbraco-cms/backend-api';
import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/element';
// eslint-disable-next-line import/order
import { UmbWorkspaceVariantContext } from '../workspace/workspace-variant/workspace-variant.context';

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
	public set property(value: PropertyTypeViewModelBaseModel | undefined) {
		this._property = value;
		this._updatePropertyVariantId();
	}

	private _variantContext?: UmbWorkspaceVariantContext;

	@state()
	private _workspaceVariantId?: UmbVariantId;

	@state()
	private _propertyVariantId?: UmbVariantId;

	constructor() {
		super();
		this.consumeContext('umbWorkspaceVariantContext', (workspaceContext: UmbWorkspaceVariantContext) => {
			this._variantContext = workspaceContext;
			this._observeVariantContext();
		});
	}

	private _observeVariantContext() {
		console.log('_observeVariantContext');
		if (!this._variantContext || !this.property) return;
		this.observe(this._variantContext.variantId, (variantId) => {
			this._workspaceVariantId = variantId;
			this._updatePropertyVariantId();
		});
	}

	private _updatePropertyVariantId() {
		console.log('_updatePropertyVariantId');
		if (this._workspaceVariantId && this.property) {
			const newVariantId = new UmbVariantId(
				this.property.variesByCulture ? this._workspaceVariantId.culture : null,
				this.property.variesBySegment ? this._workspaceVariantId.segment : null
			);
			if (!this._propertyVariantId || !newVariantId.equal(this._propertyVariantId)) {
				this._propertyVariantId = newVariantId;
			}
		}
	}

	render() {
		return html`<umb-property-type-based-property
			.property=${this._property}
			.variantId=${this._propertyVariantId}></umb-property-type-based-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-variantable-property': UmbVariantablePropertyElement;
	}
}
