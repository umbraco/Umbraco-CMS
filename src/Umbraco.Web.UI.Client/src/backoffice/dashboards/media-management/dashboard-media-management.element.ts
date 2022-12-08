import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import './media-management-grid.element';

@customElement('umb-dashboard-media-management')
export class UmbDashboardMediaManagementElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<div>HEADER</div>
			<umb-media-management-grid></umb-media-management-grid>
		`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
