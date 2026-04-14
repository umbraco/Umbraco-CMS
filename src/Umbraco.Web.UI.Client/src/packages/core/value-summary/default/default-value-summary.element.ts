import { UmbValueSummaryElementBase } from '../base/value-summary-element.base.js';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-default-value-summary')
export class UmbDefaultValueSummaryElement extends UmbValueSummaryElementBase {
	override render() {
		return html`<span>${String(this._value ?? '')}</span>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-default-value-summary': UmbDefaultValueSummaryElement;
	}
}
