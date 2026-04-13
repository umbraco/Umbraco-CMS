import { UmbValueSummaryElementBase } from '../components/value-summary-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-default-value-summary')
export class UmbDefaultValueSummaryElement extends UmbValueSummaryElementBase {
	override render() {
		return html`<span>${String(this._value ?? '')}</span>`;
	}
}

export { UmbDefaultValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-value-summary': UmbDefaultValueSummaryElement;
	}
}
