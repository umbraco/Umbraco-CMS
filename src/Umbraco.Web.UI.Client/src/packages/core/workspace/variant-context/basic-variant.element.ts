import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UmbBasicVariantContext } from './basic-variant-context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-basic-variant')
export class UmbBasicVariantElement extends UmbLitElement {
	public readonly context = new UmbBasicVariantContext(this);

	@property({ attribute: false })
	public get value(): Array<UmbPropertyValueData> {
		return this.context.getPropertyMap();
	}
	public set value(value: Array<UmbPropertyValueData>) {
		this.context.setPropertyMap(value);
	}

	@property({ attribute: false })
	public get name(): string | undefined {
		return this.context.getName();
	}
	public set name(value: string | undefined) {
		this.context.setName(value);
	}

	render() {
		return html`<slot></slot>`;
	}
}

export default UmbBasicVariantElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-basic-variant': UmbBasicVariantElement;
	}
}
