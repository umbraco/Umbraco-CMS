import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import '@umbraco-cms/backoffice/auth';

@customElement('umb-app-auth')
export class UmbAppAuthElement extends UmbLitElement {
	override render() {
		return html`<umb-auth-view></umb-auth-view>`;
	}
}

export default UmbAppAuthElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-app-auth': UmbAppAuthElement;
	}
}
