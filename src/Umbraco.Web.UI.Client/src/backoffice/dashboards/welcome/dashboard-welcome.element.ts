import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-welcome')
export class UmbDashboardWelcomeElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Welcome</h1>
				<p>You can find details about the POC in the readme.md file.</p>
			</uui-box>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-welcome': UmbDashboardWelcomeElement;
	}
}
