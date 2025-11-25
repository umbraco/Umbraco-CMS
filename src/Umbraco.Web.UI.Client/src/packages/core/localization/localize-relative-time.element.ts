import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';
import { css, customElement, html, nothing, property, state, unsafeHTML } from '@umbraco-cms/backoffice/external/lit';
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
	@property({ type: Number })
	time!: number;

	/**
	 * Formatting options
	 * @attr
	 * @example options={ dateStyle: 'full', timeStyle: 'long', timeZone: 'Australia/Sydney' }
	 */
	@property({ type: Object })
	options?: Intl.RelativeTimeFormatOptions;

	/**
	 * Unit
	 * @attr
	 * @example unit='seconds'
	 */
	@property()
	unit: Intl.RelativeTimeFormatUnit = 'seconds';

	@state()
	private _text: string | null | undefined = undefined;

	/**
	 * Computes the localized relative time when properties change or when the localization controller triggers an update.
	 * This lifecycle method runs before render and caches the result to avoid repeated computations.
	 * @param {PropertyValues} changedProperties - The properties that changed since the last update.
	 */
	protected override willUpdate(changedProperties: PropertyValues): void {
		// Update when properties change OR when localization controller triggers update
		if (
			changedProperties.has('time') ||
			changedProperties.has('unit') ||
			changedProperties.has('options') ||
			changedProperties.size === 0
		) {
			this._text = this.time ? this.localize.relativeTime(this.time, this.unit, this.options) : null;
		}
	}

	override render() {
		// undefined = not yet computed (loading), don't show fallback
		// null = no time provided, show fallback
		// string = time formatted, show it
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
		'umb-localize-relative-time': UmbLocalizeRelativeTimeElement;
	}
}
