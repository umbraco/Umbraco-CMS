import { css, html, LitElement, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';

@customElement('umb-dashboard')
export class UmbDashboardElement extends UmbElementMixin(LitElement) {
	@state()
	private _routes: UmbRoute[] = [
		{
			path: `/tab1`,
			component: () => import('./tabs/tab1.element.js'),
		},
		{
			path: `/tab2`,
			component: () => import('./tabs/tab2.element.js'),
		},
		{
			path: '',
			redirectTo: 'tab1',
		},
	];

	override render() {
		return html`
			<div>
				Dashboard 1
				<ul>
					<li><a href="section/content/dashboard/example/tab1">Tab 1</a></li>
					<li><a href="section/content/dashboard/example/tab2">Tab 2 (with modal)</a></li>
				</ul>
				<hr />
				<umb-router-slot .routes=${this._routes}></umb-router-slot>
			</div>
		`;
	}

	static override styles = [UmbTextStyles, css``];
}

export default UmbDashboardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard': UmbDashboardElement;
	}
}
