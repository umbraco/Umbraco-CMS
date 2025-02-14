import { css, customElement, nothing, property } from '@umbraco-cms/backoffice/external/lit';
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

	override updated() {
		this.#setTitle();
	}

	override render() {
		return this.date ? this.localize.date(this.date, this.options) : nothing;
	}

	#setTitle() {
		let title = '';

		if (this.date) {
			const now = new Date();
			const d = new Date(this.date);
			const duration = this.localize.duration(d, now);
			title = this.localize.term('general_duration', duration, d, now);
		}

		this.title = title;
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
