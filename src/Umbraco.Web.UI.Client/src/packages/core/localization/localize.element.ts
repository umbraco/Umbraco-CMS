import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, nothing, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { escapeHTML } from '@umbraco-cms/backoffice/utils';
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
	 * @type {unknown[] | undefined}
	 */
	@property({ type: Array })
	args?: unknown[];

	/**
	 * If true, the key will be rendered instead of the fallback value if the key is not found.
	 * @attr
	 */
	@property({ type: Boolean })
	debug = false;

	@state()
	private _text: string | null | undefined = undefined;

	/**
	 * Computes the localized text when properties change or when the localization controller triggers an update.
	 * This lifecycle method runs before render and caches the result to avoid repeated computations.
	 * @param {PropertyValues} changedProperties - The properties that changed since the last update.
	 */
	protected override willUpdate(changedProperties: PropertyValues): void {
		// Update when properties change OR when localization controller triggers update
		if (changedProperties.has('key') || changedProperties.has('args') || changedProperties.size === 0) {
			// As translated texts can contain HTML, we will need to render with unsafeHTML.
			// But arguments can come from user input, so they should be escaped.
			const escapedArgs = (this.args ?? []).map((a) => escapeHTML(a));

			this._text = this.localize.termOrDefault(this.key, null, ...escapedArgs);

			if (this._text !== null) {
				this._text = this._text.trim();
			}
		}
	}

	/**
	 * Updates the data-localize-missing attribute after the element has rendered.
	 * This attribute is set when the localization key is not found, allowing for debugging and styling.
	 * @param {PropertyValues} changedProperties - The properties that changed since the last update.
	 */
	protected override updated(changedProperties: PropertyValues): void {
		if (changedProperties.has('_text')) {
			if (this._text === null) {
				this.setAttribute('data-localize-missing', this.key);
			} else {
				this.removeAttribute('data-localize-missing');
			}
		}
	}

	override render() {
		// undefined = not yet computed (loading), don't show fallback
		// null = key not found, show fallback
		// string = translation found, show it
		if (this._text === undefined) {
			return nothing;
		}

		return this._text !== null
			? unsafeHTML(this._text)
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
