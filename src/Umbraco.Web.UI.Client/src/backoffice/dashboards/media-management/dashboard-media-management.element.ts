import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';
import './media-management-grid.element';
import '../../components/collection/collection-header.element';
import '../../components/collection/collection-selection-actions.element';
import '../../components/collection/collection-view.element';

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
				border: 1px solid black;
			}
		`,
	];

	render() {
		return html`
			<umb-collection-view>
				<umb-collection-header slot="header"></umb-collection-header>
				<umb-media-management-grid slot="main"></umb-media-management-grid>
				<umb-collection-selection-actions slot="footer"></umb-collection-selection-actions>
			</umb-collection-view>
		`;
	}
}

export default UmbDashboardMediaManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-media-management': UmbDashboardMediaManagementElement;
	}
}
