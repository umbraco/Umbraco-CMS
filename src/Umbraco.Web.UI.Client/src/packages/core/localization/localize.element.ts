import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * This element allows you to localize a string with optional interpolation values.
 * @element umb-localize
 * @slot - The fallback value if the key is not found.
 */
@customElement('umb-localize')
export class UmbLocalizeElement extends UmbLitElement {
	/**
	 * The key to localize. The key is case sensitive.
	 * @attr
	 * @example key="general_ok"
	 */
	@property({ type: String })
	key!: string;

	/**
	 * If true, the key will be rendered instead of the localized value if the key is not found.
	 * @attr
	 */
	@property()
	debug = false;

	@state()
	get text(): string {
		const localizedValue = this.localize.term(this.key);

		// If the value is the same as the key, it means the key was not found.
		if (localizedValue === this.key) {
			(this.getHostElement() as HTMLElement).setAttribute('data-localize-missing', this.key);
			return '';
		}

		(this.getHostElement() as HTMLElement).removeAttribute('data-localize-missing');

		return localizedValue;
	}

	protected render() {
		return this.text
			? html`${this.text}`
			: this.debug
			? html`<span style="color:red">${this.key}</span>`
			: html`<slot></slot>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-localize': UmbLocalizeElement;
	}
}
