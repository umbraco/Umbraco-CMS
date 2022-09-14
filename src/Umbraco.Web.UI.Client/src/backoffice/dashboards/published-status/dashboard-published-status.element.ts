import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-published-status')
export class UmbDashboardPublishedStatusElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Published Status</h1>
			</uui-box>
		`;
	}
}

export default UmbDashboardPublishedStatusElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-published-status': UmbDashboardPublishedStatusElement;
	}
}
