import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import type { UmbDocumentTypeWorkspaceViewEditTabElement } from './document-type-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { encodeFolderName, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
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
	_tabs: Array<PropertyTypeContainerModelBaseModel> = [];

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
					path: `tab/${encodeFolderName(tabName || '').toString()}`,
					component: () => import('./document-type-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).tabName = tabName ?? '';
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).ownerTabId = tab.id;
					},
				});
			});
		}

		routes.push({
			path: '',
			component: () => import('./document-type-workspace-view-edit-tab.element.js'),
			setup: (component) => {
				(component as UmbDocumentTypeWorkspaceViewEditTabElement).noTabName = true;
			},
		});

		if (routes.length !== 0) {
			routes.push({
				path: '',
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
		const tab = await this._workspaceContext?.structure.createContainer(null, null, 'Tab');
		if(tab) {
			const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
			window.history.replaceState(null, '', path);
			setTimeout(() => {
				(this.shadowRoot?.querySelector('uui-tab[active] uui-input') as (UUIInputElement | undefined))?.focus();
			}, 100);
		}
	}

	#tabNameChanged(event: InputEvent, tab: PropertyTypeContainerModelBaseModel) {

		console.log(event.target)

		let newName = (event.target as HTMLInputElement).value;

		if(newName === '') {
			newName = 'Unnamed';
		}

		const changedName = this._workspaceContext?.structure.makeContainerNameUniqueForOwnerDocument(newName, 'Tab')
		// Check if it collides with another tab name of this same document-type, if so adjust name:
		if(changedName) {
			newName = changedName;
			(event.target as HTMLInputElement).value = newName;
		}

		this._tabsStructureHelper.partialUpdateContainer(tab.id, {
			name: newName,
		});

		// Update the current URL, so we are still on this specific tab:
		window.history.replaceState(null, '', this._routerPath + '/tab/' + encodeFolderName(newName));
	}

	renderTabsNavigation() {
		const contentTabActive = this._routerPath + '/' === this._activePath;
		return html`<uui-tab-group>
				<uui-tab
					class=${this._hasRootGroups || contentTabActive ? '' : 'content-tab-is-empty'}
					label="Content"
					.active=${contentTabActive}
					href=${this._routerPath + '/'}
					>Content</uui-tab
				>
			${repeat(
				this._tabs,
				(tab) => tab.id! + tab.name,
				(tab) => {
					const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
					const tabActive = path === this._activePath;
					return html`<uui-tab label=${tab.name!} .active=${tabActive} href=${path}>
						${tabActive && this._tabsStructureHelper.isOwnerContainer(tab.id!)
							? html` <uui-input
									label="Tab name"
									look="placeholder"
									value=${tab.name!}
									placeholder="Enter a name"
									@change=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
									@blur=${(e: InputEvent) => this.#tabNameChanged(e, tab)}>
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
			<umb-body-layout header-fit-height>
				<div id="header" slot="header">
					${this._routerPath ? this.renderTabsNavigation() : ''}${this.renderActions()}
				</div>
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
				position: relative;
				display: flex;
				flex-direction: column;
				height: 100%;
				--uui-tab-background: var(--uui-color-surface);
			}

			/* TODO: This should be replaced with a general workspace bar — naming is hard */
			#header {
				width: 100%;
				display: flex;
				align-items: center;
				justify-content: space-between;
				flex-wrap: nowrap;
			}

			.content-tab-is-empty {
				height: 31px;
				align-self: center;
				border-radius: 3px;
				--uui-tab-text: var(--uui-color-text-alt);
				border: dashed 1px var(--uui-color-border-emphasis);
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
