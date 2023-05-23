import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../../document-workspace.context';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import {
	encodeFolderName,
	UmbRoute,
	UmbRouterSlotChangeEvent,
	UmbRouterSlotInitEvent,
} from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-workspace-view-edit')
export class UmbDocumentWorkspaceViewEditElement
	extends UmbLitElement
	implements UmbWorkspaceEditorViewExtensionElement
{
	//private _hasRootProperties = false;
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	_tabs: Array<PropertyTypeContainerResponseModelBaseModel> = [];

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	private _tabsStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	constructor() {
		super();

		this._tabsStructureHelper.setIsRoot(true);
		this._tabsStructureHelper.setContainerChildType('Tab');
		this.observe(this._tabsStructureHelper.containers, (tabs) => {
			this._tabs = tabs;
			this._createRoutes();
		});

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbDocumentWorkspaceContext;
			this._tabsStructureHelper.setStructureManager((workspaceContext as UmbDocumentWorkspaceContext).structure);
			this._observeRootGroups();
		});
	}

	private _observeRootGroups() {
		if (!this._workspaceContext) return;

		this.observe(
			this._workspaceContext.structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this._createRoutes();
			},
			'_observeGroups'
		);
	}

	private _createRoutes() {
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name;
				routes.push({
					path: `tab/${encodeFolderName(tabName || '').toString()}`,
					component: () => import('./document-workspace-view-edit-tab.element'),
					setup: (component) => {
						(component as any).tabName = tabName;
					},
				});
			});
		}

		if (this._hasRootGroups) {
			routes.push({
				path: '',
				component: () => import('./document-workspace-view-edit-tab.element'),
				setup: (component) => {
					(component as any).noTabName = true;
				},
			});
		}

		if (routes.length !== 0) {
			routes.push({
				path: '**',
				redirectTo: routes[0]?.path,
			});
		}

		this._routes = routes;
	}

	render() {
		return html`
			${this._routerPath && this._tabs.length > 1
				? html` <uui-tab-group>
						${this._hasRootGroups && this._tabs.length > 1
							? html`
									<uui-tab
										label="Content"
										.active=${this._routerPath + '/' === this._activePath}
										href=${this._routerPath + '/'}
										>Content</uui-tab
									>
							  `
							: ''}
						${repeat(
							this._tabs,
							(tab) => tab.name,
							(tab) => {
								const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
								return html`<uui-tab label=${tab.name!} .active=${path === this._activePath} href=${path}
									>${tab.name}</uui-tab
								>`;
							}
						)}
				  </uui-tab-group>`
				: ''}

			<umb-router-slot
				.routes=${this._routes}
				@init=${(event: UmbRouterSlotInitEvent) => {
					this._routerPath = event.target.absoluteRouterPath;
				}}
				@change=${(event: UmbRouterSlotChangeEvent) => {
					this._activePath = event.target.absoluteActiveViewPath || '';
				}}>
			</umb-router-slot>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				--uui-tab-background: var(--uui-color-surface);
			}
		`,
	];
}

export default UmbDocumentWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit': UmbDocumentWorkspaceViewEditElement;
	}
}
