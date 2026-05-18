import { customElement, html, css, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';
import type { UmbLinkPickerLink } from '../../link-picker-modal';

@customElement('umb-multi-url-picker-value-summary')
export class UmbMultiUrlPickerValueSummaryElement extends UmbValueSummaryElementBase<Array<UmbLinkPickerLink>> {
	override render() {
		if (!this._value?.length) return nothing;
		const labels = this._value.map((link) => link.name || link.url || '').filter(Boolean);
		return html`<span class="labels" title="${labels.join(', ')}">${labels.join(', ')}</span>`;
	}

	static override styles = css`
		.labels {
			display: block;
			overflow: hidden;
			text-overflow: ellipsis;
			white-space: nowrap;
			max-width: 20ch;
		}
	`;
}

export { UmbMultiUrlPickerValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-multi-url-picker-value-summary': UmbMultiUrlPickerValueSummaryElement;
	}
}
