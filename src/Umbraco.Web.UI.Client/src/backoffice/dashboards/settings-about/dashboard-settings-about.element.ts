import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-settings-about')
export class UmbDashboardSettingsAboutElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Settings</h1>
			</uui-box>
		`;
	}
}

export default UmbDashboardSettingsAboutElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-settings-about': UmbDashboardSettingsAboutElement;
	}
}
