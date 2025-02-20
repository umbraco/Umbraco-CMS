import type { UmbDashboardExamineIndexElement } from './views/section-view-examine-indexers.js';
import type { UmbDashboardExamineSearcherElement } from './views/section-view-examine-searchers.js';
import { html, css, nothing, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';

import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

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
		{
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		},
	];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	override render() {
		return html`
			<umb-body-layout header-transparent>
				${this.#renderHeader()}
				<div id="main">
					<umb-router-slot
						.routes=${this._routes}
						@init=${(event: UmbRouterSlotInitEvent) => {
							this._routerPath = event.target.absoluteRouterPath;
						}}
						@change=${(event: UmbRouterSlotChangeEvent) => {
							this._activePath = event.target.localActiveViewPath || '';
						}}></umb-router-slot>
				</div>
			</umb-body-layout>
		`;
	}

	#renderHeader() {
		return this._routerPath && this._activePath !== ''
			? html`
					<div id="header" slot="header">
						<a href=${this._routerPath}> &larr; Back to overview </a>
					</div>
				`
			: nothing;
	}

	static override styles = [
		css`
			#header {
				display: flex;
				width: 100%;
			}
			#main:not(:first-child) {
				padding-top: var(--uui-size-1);
			}
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

export { UmbDashboardExamineManagementElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-examine-management': UmbDashboardExamineManagementElement;
	}
}
