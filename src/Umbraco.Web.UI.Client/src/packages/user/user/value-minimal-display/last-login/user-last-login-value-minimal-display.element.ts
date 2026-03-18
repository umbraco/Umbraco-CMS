import { customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-user-last-login-value-minimal-display')
export class UmbUserLastLoginValueMinimalDisplayElement extends UmbLitElement {
	@property({ attribute: false })
	value?: string | null;

	override render() {
		if (!this.value) return html`<span>—</span>`;
		return html`<span>${this.localize.dateTime(this.value)}</span>`;
	}
}

export { UmbUserLastLoginValueMinimalDisplayElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-user-last-login-value-minimal-display': UmbUserLastLoginValueMinimalDisplayElement;
	}
}
