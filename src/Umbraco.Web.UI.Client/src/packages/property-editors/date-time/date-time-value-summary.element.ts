import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { DateTime } from '@umbraco-cms/backoffice/external/luxon';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

type UmbDateTimePropertyEditorValue = { date: string | null; timeZone?: string | null } | undefined;

@customElement('umb-date-time-property-editor-value-summary')
export class UmbDateTimePropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbDateTimePropertyEditorValue> {
	override render() {
		const date = this._value?.date;
		if (!date) return nothing;

		let formatted: string;
		if (this._value?.timeZone) {
			const dt = DateTime.fromISO(date, { zone: this._value.timeZone }).toLocal();
			formatted = dt.second !== 0 ? dt.toLocaleString(DateTime.DATETIME_MED_WITH_SECONDS) : dt.toFormat('ff');
		} else if (date.includes('T')) {
			const dt = DateTime.fromISO(date);
			formatted = dt.second !== 0 ? dt.toLocaleString(DateTime.DATETIME_MED_WITH_SECONDS) : dt.toFormat('ff');
		} else if (!date.includes('-')) {
			const dt = DateTime.fromFormat(date, 'HH:mm:ss');
			formatted =
				dt.second !== 0 ? dt.toLocaleString(DateTime.TIME_WITH_SECONDS) : dt.toLocaleString(DateTime.TIME_SIMPLE);
		} else {
			formatted = DateTime.fromISO(date).toFormat('DD');
		}

		return html`<span>${formatted}</span>`;
	}
}

export { UmbDateTimePropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-property-editor-value-summary': UmbDateTimePropertyEditorValueSummaryElement;
	}
}
