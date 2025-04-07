import { escapeHTML } from '../utils/index.js';
import { css, customElement, html, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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
	@property()
	key!: string;

	/**
	 * The values to forward to the localization function (must be JSON compatible).
	 * @attr
	 * @example args="[1,2,3]"
	 * @type {any[] | undefined}
	 */
	@property({ type: Array })
	args?: unknown[];

	/**
	 * If true, the key will be rendered instead of the localized value if the key is not found.
	 * @attr
	 */
	@property({ type: Boolean })
	debug = false;

	@state()
	protected get text(): string {
		// As translated texts can contain HTML, we will need to render with unsafeHTML.
		// But arguments can come from user input, so they should be escaped.
		const escapedArgs = (this.args ?? []).map((a) => escapeHTML(a));

		const localizedValue = this.localize.term(this.key, ...escapedArgs);

		// If the value is the same as the key, it means the key was not found.
		if (localizedValue === this.key) {
			(this.getHostElement() as HTMLElement).setAttribute('data-localize-missing', this.key);
			return '';
		}

		(this.getHostElement() as HTMLElement).removeAttribute('data-localize-missing');

		return localizedValue;
	}

	override render() {
		return this.text.trim()
			? html`${unsafeHTML(this.text)}`
			: this.debug
				? html`<span style="color:red">${this.key}</span>`
				: html`<slot></slot>`;
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
		'umb-localize': UmbLocalizeElement;
	}
}
