import { css, customElement, html, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * This element allows you to localize a number
 * @element umb-localize-number
 * @slot - The fallback value if the key is not found.
 */
@customElement('umb-localize-number')
export class UmbLocalizeNumberElement extends UmbLitElement {
	/**
	 * The number to localize.
	 * @attr
	 * @example number=1_000_000
	 */
	@property()
	number!: number | string;

	/**
	 * Formatting options
	 * @attr
	 * @example options={ style: 'currency', currency: 'EUR' }
	 */
	@property()
	options?: Intl.NumberFormatOptions;

	@state()
	protected get text(): string {
		return this.localize.number(this.number, this.options);
	}

	override render() {
		return this.number ? html`${unsafeHTML(this.text)}` : html`<slot></slot>`;
	}

	static override styles = [
		css`
			:host {
				display: contents;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize-number': UmbLocalizeNumberElement;
	}
}
