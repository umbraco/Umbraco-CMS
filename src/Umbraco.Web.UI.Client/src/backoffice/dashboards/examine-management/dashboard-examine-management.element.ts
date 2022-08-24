import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-examine-management')
export class UmbDashboardExamineManagementElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Examine Management</h1>
			</uui-box>
		`;
	}
}

export default UmbDashboardExamineManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-management': UmbDashboardExamineManagementElement;
	}
}
