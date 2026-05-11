import { customElement, html, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbValueSummaryElementBase } from '@umbraco-cms/backoffice/value-summary';

@customElement('umb-multiple-text-string-value-summary')
export class UmbMultipleTextStringValueSummaryElement extends UmbValueSummaryElementBase<Array<string>> {
	override render() {
		if (!this._value?.length) return nothing;
		return html`<span>${this._value.join(', ')}</span>`;
	}
}

export { UmbMultipleTextStringValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-multiple-text-string-value-summary': UmbMultipleTextStringValueSummaryElement;
	}
}
