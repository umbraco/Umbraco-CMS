import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UmbBasicVariantContext } from './basic-variant-context.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-basic-variant')
export class UmbBasicVariantElement extends UmbLitElement {
	// A take on only firing events when the value is changed from the outside.
	#silent = false;

	public readonly context: UmbBasicVariantContext;

	@property({ attribute: false })
	public get value(): Array<UmbPropertyValueData> {
		return this.context.getValues();
	}
	public set value(value: Array<UmbPropertyValueData>) {
		this.#silent = true;
		this.context.setValues(value);
		this.#silent = false;
	}

	@property({ attribute: false })
	public get name(): string | undefined {
		return this.context.getName();
	}
	public set name(value: string | undefined) {
		this.#silent = true;
		this.context.setName(value);
		this.#silent = false;
	}

	constructor() {
		super();
		this.context = new UmbBasicVariantContext(this);
		this.observe(this.context.name, () => {
			if (!this.#silent) {
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
		this.observe(this.context.values, () => {
			console.log('value change');
			if (!this.#silent) {
				console.log('value fire event!');
				this.dispatchEvent(new UmbChangeEvent());
			}
		});
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
