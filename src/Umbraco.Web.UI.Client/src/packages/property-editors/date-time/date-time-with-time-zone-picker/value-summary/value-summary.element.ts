import type { UmbDateTimeWithTimeZonePropertyEditorValue } from '../types.js';
import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-date-time-with-time-zone-property-editor-value-summary')
export class UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbDateTimeWithTimeZonePropertyEditorValue> {
	override render() {
		if (!this._value?.date || !this._value?.timeZone) return nothing;
		// TODO: Replace Luxon with `Temporal.PlainDateTime.from(date).toZonedDateTime(timeZone).toLocaleString()` once Temporal is available in all browsers.
		const localDate = DateTime.fromISO(this._value.date, { zone: this._value.timeZone }).toLocal();
		return html`<span>${localDate.toFormat('ff')}</span>`;
	}
}

export { UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-with-time-zone-property-editor-value-summary': UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement;
	}
}
