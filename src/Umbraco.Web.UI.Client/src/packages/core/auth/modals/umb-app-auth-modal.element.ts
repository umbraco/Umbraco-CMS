import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-app-auth-modal')
export class UmbAppAuthModalElement extends UmbLitElement {
	render() {
		return html`<h1>Umb App Auth Modal</h1>`;
	}
}

export default UmbAppAuthModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-auth-modal': UmbAppAuthModalElement;
	}
}
