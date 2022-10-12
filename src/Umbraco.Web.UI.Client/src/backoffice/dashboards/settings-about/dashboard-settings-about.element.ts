import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-dashboard-settings-about')
export class UmbDashboardSettingsAboutElement extends LitElement {
	static styles = [UUITextStyles, css``];

	render() {
		return html`
			<uui-box>
				<h1>Start here</h1>
				<p>This section contains the building blocks for your Umbraco site. Follow the below links to find out more about working with the items in the Settings section.</p>
				<h2>Find out more:</h2>
				<ul>
					<li>Read more about working with the items in Settings <a href="https://our.umbraco.com/documentation/Getting-Started/Backoffice/Sections/" target="_blank" rel="noopener">in the Documentation section</a> of Our Umbraco</li>
					<li>Ask a question in the <a href="https://our.umbraco.com/forum" target="_blank" rel="noopener">Community Forum</a></li>
					<li>Watch our free <a href="https://umbra.co/ulb" target="_blank" rel="noopener">tutorial videos on the Umbraco Learning Base</a></li>
					<li>Find out about our <a href="https://umbraco.com/products/" target="_blank" rel="noopener">productivity boosting tools and commercial support</a></li>
					<li>Find out about real-life <a href="https://umbraco.com/training/" target="_blank" rel="noopener">training and certification</a> opportunities</li>
				</ul>
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
