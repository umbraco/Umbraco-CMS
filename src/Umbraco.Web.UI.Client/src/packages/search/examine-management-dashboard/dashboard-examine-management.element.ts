import { html, css, nothing } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbDashboardExamineIndexElement } from './views/section-view-examine-indexers.js';
import { UmbDashboardExamineSearcherElement } from './views/section-view-examine-searchers.js';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-dashboard-examine-management')
export class UmbDashboardExamineManagementElement extends UmbLitElement {
	@state()
	private _routes: UmbRoute[] = [
		{
			path: `/index/:indexerName`,
			component: () => import('./views/section-view-examine-indexers.js'),
			setup: (component, info) => {
				const element = component as UmbDashboardExamineIndexElement;
				element.indexName = info.match.params.indexerName;
			},
		},
		{
			path: `/searcher/:searcherName`,
			component: () => import('./views/section-view-examine-searchers.js'),
			setup: (component, info) => {
				const element = component as UmbDashboardExamineSearcherElement;
				element.searcherName = info.match.params.searcherName;
			},
		},
		{
			path: ``,
			component: () => import('./views/section-view-examine-overview.js'),
		},
	];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	render() {
		return html` ${this._routerPath && this._activePath !== ''
				? html` <a href=${this._routerPath}> &larr; Back to overview </a> `
				: nothing}
			<umb-router-slot
				.routes=${this._routes}
				@init=${(event: UmbRouterSlotInitEvent) => {
					this._routerPath = event.target.absoluteRouterPath;
				}}
				@change=${(event: UmbRouterSlotChangeEvent) => {
					this._activePath = event.target.localActiveViewPath || '';
				}}></umb-router-slot>`;
	}

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
}

export default UmbDashboardExamineManagementElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-management': UmbDashboardExamineManagementElement;
	}
}
