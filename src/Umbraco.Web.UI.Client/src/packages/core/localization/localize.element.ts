import { css, customElement, html, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * This element allows you to localize a string with optional interpolation values.
 * @element umb-localize
 * @slot - The fallback value if the key is not found.
 */
@customElement('umb-localize')
export class UmbLocalizeElement extends UmbLitElement {
	/**
	 * The key to localize. The key is case sensitive. For non-term localizations, this is the input
	 * @attr
	 * @example key="general_ok"
	 */
	@property()
	key!: string;

	/**
	 * The values to forward to the localization function (must be JSON compatible).
	 * @attr
	 * @example args="[1,2,3]"
	 * @type {any[] | undefined}
	 */
	@property({
		type: Array,
	})
	args?: unknown[];

	/**
	 * If true, the key will be rendered instead of the localized value if the key is not found.
	 * @attr
	 */
	@property()
	debug = false;

	@property()
	type: 'term' | 'date' | 'number' | 'relativeTime' = 'term';

	@property()
	unit: Intl.RelativeTimeFormatUnit = 'seconds';

	@property() 
	options?: Intl.DateTimeFormatOptions | Intl.NumberFormatOptions | Intl.RelativeTimeFormatOptions;
	
	@state()
	protected get text(): string {
		let localizedValue = '';

		switch (this.type) {
			case 'term':
				localizedValue = this.localize.term(this.key, ...(this.args ?? []));
				break;
			case 'date':
				localizedValue = this.localize.date(this.key, this.options as Intl.DateTimeFormatOptions);
				break;
			case 'number':
				localizedValue = this.localize.number(this.key, this.options as Intl.NumberFormatOptions);
				break;				
			case 'relativeTime':
				localizedValue = this.localize.relativeTime(+this.key, this.unit, this.options as Intl.RelativeTimeFormatOptions);
				break;		
			default:
				throw('unsupported type')
				break;
		}
		// If the value is the same as the key, it means the key was not found.
		if (localizedValue === this.key) {
			(this.getHostElement() as HTMLElement).setAttribute('data-localize-missing', this.key);
			return '';
		}

		(this.getHostElement() as HTMLElement).removeAttribute('data-localize-missing');

		return localizedValue;
	}

	protected render() {
		return this.text.trim()
			? html`${unsafeHTML(this.text)}`
			: this.debug
				? html`<span style="color:red">${this.key}</span>`
				: html`<slot></slot>`;
	}

	static styles = [
		css`
			:host {
				display: contents;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
