import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbLinkPickerLink } from '../../link-picker-modal';

@customElement('umb-multi-url-picker-value-summary')
export class UmbMultiUrlPickerValueSummaryElement extends UmbValueSummaryElementBase<Array<UmbLinkPickerLink>> {
	override render() {
		if (!this._value?.length) return nothing;
		const labels = this._value.map((link) => link.name || link.url || '').filter(Boolean);
		return html`<span style="white-space: nowrap">${labels.join(', ')}</span>`;
	}
}

export { UmbMultiUrlPickerValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-multi-url-picker-value-summary': UmbMultiUrlPickerValueSummaryElement;
	}
}
