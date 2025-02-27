import { css, customElement, html, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * This element allows you to localize a date
 * @element umb-localize-date
 * @slot - The fallback value if the key is not found.
 */
@customElement('umb-localize-date')
export class UmbLocalizeDateElement extends UmbLitElement {
	/**
	 * The date to localize.
	 * @attr
	 * @example date="Sep 22 2023"
	 */
	@property({ type: String })
	date?: string | Date;

	/**
	 * Formatting options
	 * @attr
	 * @example options={ dateStyle: 'full', timeStyle: 'long', timeZone: 'Australia/Sydney' }
	 */
	@property({ type: Object })
	options?: Intl.DateTimeFormatOptions;

	@state()
	protected get text(): string {
		return this.localize.date(this.date!, this.options);
	}

	override render() {
		return this.date ? html`${unsafeHTML(this.text)}` : html`<slot></slot>`;
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
		'umb-localize-date': UmbLocalizeDateElement;
	}
}
