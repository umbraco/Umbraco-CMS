import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, nothing, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
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
	@property({ type: Object })
	options?: Intl.NumberFormatOptions;

	@state()
	private _text: string | null | undefined = undefined;

	/**
	 * Computes the localized number when properties change or when the localization controller triggers an update.
	 * This lifecycle method runs before render and caches the result to avoid repeated computations.
	 * @param {PropertyValues} changedProperties - The properties that changed since the last update.
	 */
	protected override willUpdate(changedProperties: PropertyValues): void {
		// Update when properties change OR when localization controller triggers update
		if (changedProperties.has('number') || changedProperties.has('options') || changedProperties.size === 0) {
			this._text = this.number ? this.localize.number(this.number, this.options) : null;
		}
	}

	override render() {
		// undefined = not yet computed (loading), don't show fallback
		// null = no number provided, show fallback
		// string = number formatted, show it
		if (this._text === undefined) {
			return nothing;
		}

		return this._text !== null ? html`${unsafeHTML(this._text)}` : html`<slot></slot>`;
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
