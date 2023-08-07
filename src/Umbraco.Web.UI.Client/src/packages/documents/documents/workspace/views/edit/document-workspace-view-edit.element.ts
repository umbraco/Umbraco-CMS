import { UmbDocumentWorkspaceContext } from '../../document-workspace.context.js';
import type { UmbDocumentWorkspaceViewEditTabElement } from './document-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import {
	encodeFolderName,
	UmbRoute,
	UmbRouterSlotChangeEvent,
	UmbRouterSlotInitEvent,
} from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
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
	_tabs?: Array<PropertyTypeContainerModelBaseModel>;

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

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
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
		if (!this._tabs || !this._workspaceContext) return;
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName).toString()}`,
					component: () => import('./document-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbDocumentWorkspaceViewEditTabElement).tabName = tabName;
						// TODO: Consider if we can link these more simple, and not parse this on.
						// Instead have the structure manager looking at wether one of the OwnerALikecontainers is in the owner document.
						(component as UmbDocumentWorkspaceViewEditTabElement).ownerTabId =
							this._workspaceContext?.structure.isOwnerContainer(tab.id!) ? tab.id : undefined;
					},
				});
			});
		}

		if (this._hasRootGroups) {
			routes.push({
				path: '',
				component: () => import('./document-workspace-view-edit-tab.element.js'),
				setup: (component) => {
					(component as UmbDocumentWorkspaceViewEditTabElement).noTabName = true;
					(component as UmbDocumentWorkspaceViewEditTabElement).ownerTabId = null;
				},
			});
		}

		if (routes.length !== 0) {
			routes.push({
				path: '',
				redirectTo: routes[0]?.path,
			});
		}

		this._routes = routes;
	}

	render() {
		if (!this._routes || !this._tabs) return;
		return html`
			<umb-body-layout header-fit-height>
				${this._routerPath && (this._tabs.length > 0 || this._hasRootGroups)
					? html` <uui-tab-group slot="header">
							${this._hasRootGroups && this._tabs.length > 0
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
									return html`<uui-tab label=${tab.name ?? 'unnanmed'} .active=${path === this._activePath} href=${path}
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
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				height: 100%;
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
