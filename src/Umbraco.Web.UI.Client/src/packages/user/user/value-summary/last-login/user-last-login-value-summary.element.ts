import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-last-login-value-summary')
export class UmbUserLastLoginValueSummaryElement extends UmbLitElement {
	@property({ attribute: false })
	value?: string | null;

	override render() {
		if (!this.value) return html`<span>—</span>`;
		return html`<span>${this.localize.dateTime(this.value)}</span>`;
	}
}

export { UmbUserLastLoginValueSummaryElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-last-login-value-summary': UmbUserLastLoginValueSummaryElement;
	}
}
