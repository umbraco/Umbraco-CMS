import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import './media-management-grid.element';
import '../../components/list-view/list-view-header.element';
import '../../components/list-view/list-view-selection.element';

@customElement('umb-dashboard-media-management')
export class UmbDashboardMediaManagementElement extends LitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				box-sizing: border-box;
				gap: var(--uui-size-space-5);
				height: 100%;
			}
		`,
	];

	render() {
		return html`
			<umb-list-view-header></umb-list-view-header>
			<umb-media-management-grid></umb-media-management-grid>
			<umb-list-view-selection></umb-list-view-selection>
		`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
