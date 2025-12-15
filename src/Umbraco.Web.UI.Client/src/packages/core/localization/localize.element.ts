import { css, customElement, html, property, state, unsafeHTML, when } from '@umbraco-cms/backoffice/external/lit';
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
	protected get text(): string | null {
		// As translated texts can contain HTML, we will need to render with unsafeHTML.
		// But arguments can come from user input, so they should be escaped.
		const escapedArgs = (this.args ?? []).map((a) => escapeHTML(a));

		const localizedValue = this.localize.termOrDefault(this.key, null, ...escapedArgs);

		// Update the data attribute based on whether the key was found
		if (localizedValue === null) {
			(this.getHostElement() as HTMLElement).setAttribute('data-localize-missing', this.key);
			return null;
		}

		(this.getHostElement() as HTMLElement).removeAttribute('data-localize-missing');

		return localizedValue.trim();
	}

	override render() {
		return when(
			this.text,
			(text) => unsafeHTML(text),
			() => (this.debug ? html`<span style="color:red">${this.key}</span>` : html`<slot></slot>`),
		);
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
