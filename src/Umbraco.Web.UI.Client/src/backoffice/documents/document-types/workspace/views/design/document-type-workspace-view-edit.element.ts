import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context';
import type { UmbDocumentTypeWorkspaceViewEditTabElement } from './document-type-workspace-view-edit-tab.element';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type { UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/internal/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-type-workspace-view-edit')
export class UmbDocumentTypeWorkspaceViewEditElement
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

	private _workspaceContext?: UmbDocumentTypeWorkspaceContext;

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
			this._workspaceContext = workspaceContext as UmbDocumentTypeWorkspaceContext;
			this._tabsStructureHelper.setStructureManager((workspaceContext as UmbDocumentTypeWorkspaceContext).structure);
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
					path: `tab/${encodeURI(tabName || '').toString()}`,
					component: () => import('./document-type-workspace-view-edit-tab.element'),
					setup: (component) => {
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).tabName = tabName ?? '';
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).ownerTabId = tab.id;
					},
				});
			});
		}

		if (this._hasRootGroups) {
			routes.push({
				path: '',
				component: () => import('./document-type-workspace-view-edit-tab.element'),
				setup: (component) => {
					(component as UmbDocumentTypeWorkspaceViewEditTabElement).noTabName = true;
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

	#requestRemoveTab(tabId: string | undefined) {
		// TODO: If this tab is composed of other tabs, then notify that it will only delete the local tab.
		// TODO: Update URL when removing tab.
		this.#remove(tabId);
	}
	#remove(tabId: string | undefined) {
		if (!tabId) return;
		this._workspaceContext?.structure.removeContainer(null, tabId);
	}
	async #addTab() {
		this._workspaceContext?.structure.createContainer(null, null, 'Tab');
	}

	renderTabsNavigation() {
		return html`<uui-tab-group>
			${this._hasRootGroups
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
				(tab) => tab.id! + tab.name,
				(tab) => {
					// TODO: make better url folder name:
					const path = this._routerPath + '/tab/' + encodeURI(tab.name || '');
					return html`<uui-tab label=${tab.name!} .active=${path === this._activePath} href=${path}>
						${path === this._activePath && this._tabsStructureHelper.isOwnerContainer(tab.id!)
							? html` <uui-input
									label="Tab name"
									look="placeholder"
									value=${tab.name!}
									placeholder="Enter a name"
									@change=${(e: InputEvent) => {
										const newName = (e.target as HTMLInputElement).value;
										this._tabsStructureHelper.partialUpdateContainer(tab.id, {
											name: newName,
										});

										// Update the current URL, so we are still on this specific tab:
										window.history.replaceState(null, '', this._routerPath + '/tab/' + encodeURI(newName));
									}}>
									<uui-button
										label="Remove tab"
										class="trash"
										slot="append"
										@click=${() => this.#requestRemoveTab(tab.id)}
										compact>
										<uui-icon name="umb:trash"></uui-icon>
									</uui-button>
							  </uui-input>`
							: tab.name}
					</uui-tab>`;
				}
			)}
			<uui-button id="add-tab" @click="${this.#addTab}" label="Add tab" compact>
				<uui-icon name="umb:add"></uui-icon>
				Add tab
			</uui-button>
		</uui-tab-group>`;
	}

	renderActions() {
		return html`<div class="tab-actions">
			<uui-button label="Compositions" look="outline" compact>
				<uui-icon name="umb:merge"></uui-icon>
				Compositions
			</uui-button>
			<uui-button label="Recorder" look="outline" compact>
				<uui-icon name="umb:navigation"></uui-icon>
				Recorder
			</uui-button>
		</div>`;
	}

	render() {
		return html`
			<div id="workspace-tab-bar">${this._routerPath ? this.renderTabsNavigation() : ''}${this.renderActions()}</div>

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

			/* TODO: This should be replaced with a general workspace bar — naming is hard */
			#workspace-tab-bar {
				padding: 0 var(--uui-size-layout-1);
				display: flex;
				align-items: center;
				justify-content: space-between;
				background-color: var(--uui-color-surface);
				flex-wrap: nowrap;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-edit': UmbDocumentTypeWorkspaceViewEditElement;
	}
}
