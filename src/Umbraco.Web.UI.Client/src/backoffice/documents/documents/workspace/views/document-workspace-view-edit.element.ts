import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { IRoute } from 'router-slot';
import { UmbDocumentWorkspaceContext } from '../document-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import { PropertyTypeContainerViewModelBaseModel } from '@umbraco-cms/backend-api';
import { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/router';

@customElement('umb-document-workspace-view-edit')
export class UmbDocumentWorkspaceViewEditElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				margin: var(--uui-size-layout-1);
			}
		`,
	];

	@state()
	private _routes: IRoute[] = [];

	//@state()
	//_tabs: PropertyTypeContainerViewModelBaseModel[] = [];

	@state()
	_tabs: Array<PropertyTypeContainerViewModelBaseModel> = [];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();

		// TODO: Figure out how to get the magic string for the workspace context.
		this.consumeContext<UmbDocumentWorkspaceContext>('umbWorkspaceContext', (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this._observeTabs();
		});
	}

	private _observeTabs() {
		if (!this._workspaceContext) return;

		this.observe(
			this._workspaceContext.rootContainers('Tab'),
			(tabs) => {
				tabs.forEach((tab) => {
					// Only add each tab name once, as our containers merge on name:
					if (!this._tabs.find((x) => x.name === tab.name || '')) {
						this._tabs.push(tab);
					}
				});
				this._createRoutes();
			},
			'_observeTabs'
		);
	}

	private _createRoutes() {
		const routes: any[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name;
				routes.push({
					path: `tab/${encodeURI(tabName || '').toString()}`,
					component: () => import('./document-workspace-view-edit-tab.element'),
					setup: (component: Promise<HTMLElement>) => {
						(component as any).tabName = tabName;
					},
				});
			});

			routes.push({
				path: '',
				redirectTo: routes[0]?.path,
			});
			routes.push({
				path: '**',
				redirectTo: routes[0]?.path,
			});
		}

		this._routes = routes;
	}

	render() {
		return html`
			<uui-tab-group>
				${repeat(
					this._tabs,
					(tab) => tab.name,
					(tab) => {
						const path = this._routerPath + '/tab/' + encodeURI(tab.name || '');
						return html`<uui-tab label=${tab.name!} .active=${path === this._activePath} href=${path}
							>${tab.name}</uui-tab
						>`;
					}
				)}
			</uui-tab-group>

			<umb-router-slot
				.routes=${this._routes}
				@init=${(event: UmbRouterSlotInitEvent) => {
					this._routerPath = event.target.absoluteRouterPath;
				}}
				@change=${(event: UmbRouterSlotChangeEvent) => {
					this._activePath = event.target.localActiveViewPath || '';
				}}>
			</umb-router-slot>
		`;
	}
}

export default UmbDocumentWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit': UmbDocumentWorkspaceViewEditElement;
	}
}
