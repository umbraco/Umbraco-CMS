import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-telemetry')
export class UmbDashboardTelemetryElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Telemetry</h1>
			</uui-box>
		`;
	}
}

export default UmbDashboardTelemetryElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-telemetry': UmbDashboardTelemetryElement;
	}
}
