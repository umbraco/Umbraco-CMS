import { html, LitElement, css, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { IRoute, IRoutingInfo, path } from 'router-slot';

import { UmbDashboardExamineIndexElement } from './views/section-view-examine-indexers';
import { UmbDashboardExamineSearcherElement } from './views/section-view-examine-searchers';

import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-dashboard-examine-management')
export class UmbDashboardExamineManagementElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		css`
			a {
				color: var(--uui-color-text);
				background: transparent;
				border: none;
				text-decoration: underline;
				cursor: pointer;
				display: inline-block;
			}
		`,
	];
	@state()
	private _routes: IRoute[] = [
		{
			path: `/index/:indexerName`,
			component: () => import('./views/section-view-examine-indexers'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbDashboardExamineIndexElement;
				element.indexName = info.match.params.indexerName;
			},
		},
		{
			path: `/searcher/:searcherName`,
			component: () => import('./views/section-view-examine-searchers'),
			setup: (component: HTMLElement, info: IRoutingInfo) => {
				const element = component as UmbDashboardExamineSearcherElement;
				element.searcherName = info.match.params.searcherName;
			},
		},
		{
			path: ``,
			component: () => import('./views/section-view-examine-overview'),
		},
	];

	@state()
	private _currentPath?: string;

	/**
	 *
	 */
	constructor() {
		super();
	}

	private _onRouteChange() {
		this._currentPath = path();
	}

	private get backbutton(): boolean {
		return !(this._currentPath?.endsWith('examine-management/'));
	}

	render() {
		return html` ${this.backbutton
				? html` <a href="section/settings/dashboard/examine-management"> &larr; Back to overview </a> `
				: nothing}
			<router-slot @changestate="${this._onRouteChange}" .routes=${this._routes}></router-slot>`;
	}
}

export default UmbDashboardExamineManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-management': UmbDashboardExamineManagementElement;
	}
}
