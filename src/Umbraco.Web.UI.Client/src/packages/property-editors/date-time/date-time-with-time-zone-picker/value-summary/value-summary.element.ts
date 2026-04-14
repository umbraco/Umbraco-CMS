import type { UmbDateTimeWithTimeZonePropertyEditorValue } from '../value-type/types.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-date-time-with-time-zone-property-editor-value-summary')
export class UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement extends UmbValueSummaryElementBase<UmbDateTimeWithTimeZonePropertyEditorValue> {
	override render() {
		return html`<span>${this._value?.date}</span>`;
	}
}

export { UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-date-time-with-time-zone-property-editor-value-summary': UmbDateTimeWithTimeZonePropertyEditorValueSummaryElement;
	}
}
