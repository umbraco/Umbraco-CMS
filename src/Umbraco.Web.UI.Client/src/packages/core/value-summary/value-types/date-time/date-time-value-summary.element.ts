import { UmbValueSummaryElementBase } from '../../base/value-summary-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-date-time-value-summary')
export class UmbDateTimeValueSummaryElement extends UmbValueSummaryElementBase<string | null> {
	override render() {
		if (!this._value) return html`<span>—</span>`;
		return html`<span>${this.localize.dateTime(this._value)}</span>`;
	}
}

export { UmbDateTimeValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-value-summary': UmbDateTimeValueSummaryElement;
	}
}
