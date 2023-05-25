import { css, html, LitElement , customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-umbraco-news-dashboard')
export class UmbUmbracoNewsDashboardElement extends LitElement {
	render() {
		return html`
			<uui-box>
				<h1>Welcome</h1>
				<p>You can find details about the POC in the readme.md file.</p>
			</uui-box>
		`;
	}

	static styles = [
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbUmbracoNewsDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-umbraco-news-dashboard': UmbUmbracoNewsDashboardElement;
	}
}
