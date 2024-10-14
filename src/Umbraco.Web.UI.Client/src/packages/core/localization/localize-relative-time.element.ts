import { css, customElement, html, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * This element allows you to localize a relative time
 * @element umb-localize-relative-time
 * @slot - The fallback value if the key is not found.
 */
@customElement('umb-localize-relative-time')
export class UmbLocalizeRelativeTimeElement extends UmbLitElement {
	/**
	 * The date to localize.
	 * @attr
	 * @example time=10
	 */
	@property()
	time!: number;

	/**
	 * Formatting options
	 * @attr
	 * @example options={ dateStyle: 'full', timeStyle: 'long', timeZone: 'Australia/Sydney' }
	 */
	@property()
	options?: Intl.RelativeTimeFormatOptions;

	/**
	 * Unit
	 * @attr
	 * @example unit='seconds'
	 */
	@property()
	unit: Intl.RelativeTimeFormatUnit = 'seconds';

	@state()
	protected get text(): string {
		return this.localize.relativeTime(this.time, this.unit, this.options);
	}

	override render() {
		return this.time ? html`${unsafeHTML(this.text)}` : html`<slot></slot>`;
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
		'umb-localize-relative-time': UmbLocalizeRelativeTimeElement;
	}
}
