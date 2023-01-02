import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-health-check')
export class UmbDashboardHealthCheckElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Health Check</h1>
			</uui-box>
		`;
	}
}

export default UmbDashboardHealthCheckElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-health-check': UmbDashboardHealthCheckElement;
	}
}
