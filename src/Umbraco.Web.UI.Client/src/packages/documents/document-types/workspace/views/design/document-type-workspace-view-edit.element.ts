import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import type { UmbDocumentTypeWorkspaceViewEditTabElement } from './document-type-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, nothing, query } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement, UUIInputEvent, UUITabElement, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { encodeFolderName, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceEditorViewExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbConfirmModalData } from '@umbraco-cms/backoffice/modal';

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
	_tabs?: Array<PropertyTypeContainerModelBaseModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	@state()
	private _buttonDisabled: boolean = false;

	private _workspaceContext?: UmbDocumentTypeWorkspaceContext;

	private _tabsStructureHelper = new UmbContentTypeContainerStructureHelper(this);

	private _modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT_TOKEN.TYPE;

	@query('uui-tab')
	private _tabElements!: UUITabElement[];

	constructor() {
		super();

		//TODO: We need to differentiate between local and composition tabs (and hybrids)

		this._tabsStructureHelper.setIsRoot(true);
		this._tabsStructureHelper.setContainerChildType('Tab');
		this.observe(this._tabsStructureHelper.containers, (tabs) => {
			this._tabs = tabs;
			this._createRoutes();
		});

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbDocumentTypeWorkspaceContext;
			this._tabsStructureHelper.setStructureManager((workspaceContext as UmbDocumentTypeWorkspaceContext).structure);
			this._observeRootGroups();
		});

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (context) => {
			this._modalManagerContext = context;
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
			'_observeGroups',
		);
	}

	private _createRoutes() {
		if (!this._workspaceContext || !this._tabs) return;
		const routes: UmbRoute[] = [];

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name ?? '';
				routes.push({
					path: `tab/${encodeFolderName(tabName).toString()}`,
					component: () => import('./document-type-workspace-view-edit-tab.element.js'),
					setup: (component) => {
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).tabName = tabName;
						(component as UmbDocumentTypeWorkspaceViewEditTabElement).ownerTabId =
							this._workspaceContext?.structure.isOwnerContainer(tab.id!) ? tab.id : undefined;
					},
				});
			});
		}

		routes.push({
			path: 'root',
			component: () => import('./document-type-workspace-view-edit-tab.element.js'),
			setup: (component) => {
				(component as UmbDocumentTypeWorkspaceViewEditTabElement).noTabName = true;
				(component as UmbDocumentTypeWorkspaceViewEditTabElement).ownerTabId = null;
			},
		});

		if (this._hasRootGroups) {
			routes.push({
				path: '',
				redirectTo: 'root',
			});
		} else if (routes.length !== 0) {
			routes.push({
				path: '',
				redirectTo: routes[0]?.path,
			});
		}

		this._routes = routes;
	}

	#requestRemoveTab(tab: PropertyTypeContainerModelBaseModel | undefined) {
		const Message: UmbConfirmModalData = {
			headline: 'Delete tab',
			content: html`<umb-localize key="contentTypeEditor_confirmDeleteTabMessage" .args=${[tab?.name ?? tab?.id]}>Are you sure you want to delete the tab <strong>${tab?.name ?? tab?.id}</strong></umb-localize>
				<div style="color:var(--uui-color-danger-emphasis)">
					<umb-localize key="contentTypeEditor_confirmDeleteTabNotice">This will delete all items that doesn't belong to a composition.</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		};

		// TODO: If this tab is composed of other tabs, then notify that it will only delete the local tab.

		const modalHandler = this._modalManagerContext?.open(UMB_CONFIRM_MODAL, Message);

		modalHandler?.onSubmit().then(() => {
			this.#remove(tab?.id);
		});
	}
	#remove(tabId?: string) {
		if (!tabId) return;
		this._workspaceContext?.structure.removeContainer(null, tabId);
		this._tabsStructureHelper?.isOwnerContainer(tabId)
			? window.history.replaceState(null, '', this._routerPath + this._routes[0]?.path ?? '/root')
			: '';
	}
	async #addTab() {
		if (
			(this.shadowRoot?.querySelector('uui-tab[active] uui-input') as UUIInputElement) &&
			(this.shadowRoot?.querySelector('uui-tab[active] uui-input') as UUIInputElement).value === ''
		) {
			this.#focusInput();
			return;
		}

		const tab = await this._workspaceContext?.structure.createContainer(null, null, 'Tab');
		if (tab) {
			const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
			window.history.replaceState(null, '', path);
			this.#focusInput();
		}
	}

	async #focusInput() {
		setTimeout(() => {
			(this.shadowRoot?.querySelector('uui-tab[active] uui-input') as UUIInputElement | undefined)?.focus();
		}, 100);
	}

	async #tabNameChanged(event: InputEvent, tab: PropertyTypeContainerModelBaseModel) {
		if (this._buttonDisabled) this._buttonDisabled = !this._buttonDisabled;
		let newName = (event.target as HTMLInputElement).value;

		if (newName === '') {
			newName = 'Unnamed';
			(event.target as HTMLInputElement).value = 'Unnamed';
		}

		const changedName = this._workspaceContext?.structure.makeContainerNameUniqueForOwnerDocument(
			newName,
			'Tab',
			tab.id,
		);

		// Check if it collides with another tab name of this same document-type, if so adjust name:
		if (changedName) {
			newName = changedName;
			(event.target as HTMLInputElement).value = newName;
		}

		this._tabsStructureHelper.partialUpdateContainer(tab.id!, {
			name: newName,
		});

		// Update the current URL, so we are still on this specific tab:
		window.history.replaceState(null, '', this._routerPath + '/tab/' + encodeFolderName(newName));
	}

	renderTabsNavigation() {
		if (!this._tabs) return;
		const rootTabPath = this._routerPath + '/root';
		const rootTabActive = rootTabPath === this._activePath;
		return html`<uui-tab-group>
				<uui-tab
					class=${this._hasRootGroups || rootTabActive ? '' : 'content-tab-is-empty'}
					label="Content"
					.active=${rootTabActive}
					href=${rootTabPath}>
					Content
				</uui-tab>
				${repeat(
					this._tabs,
					(tab) => tab.id! + tab.name,
					(tab) => {
						const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
						const tabActive = path === this._activePath;
						return html`<uui-tab label=${tab.name ?? 'unnamed'} .active=${tabActive} href=${path}>
							<div class="tab">
								${!this._tabsStructureHelper.isOwnerContainer(tab.id!)
									? html`<uui-icon class="external" name="umb:merge"></uui-icon> `
									: nothing}
								${tabActive && this._tabsStructureHelper.isOwnerContainer(tab.id!)
									? html`<uui-input
											id="input"
											label="Tab name"
											look="placeholder"
											value="${tab.name!}"
											placeholder="Unnamed"
											@change=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
											@blur=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
											@input=${() => (this._buttonDisabled = true)}
											@focus=${(e: UUIInputEvent) => (e.target.value ? nothing : (this._buttonDisabled = true))}
											auto-width>
											<uui-button
												label="Remove tab"
												class="trash"
												slot="append"
												?disabled=${this._buttonDisabled}
												@click=${() => this.#requestRemoveTab(tab)}
												compact>
												<uui-icon name="umb:trash"></uui-icon>
											</uui-button>
									  </uui-input>`
									: html`<div class="no-edit">
											${tab.name!}
											${this._tabsStructureHelper.isOwnerContainer(tab.id!)
												? html`<uui-button
														label="Remove tab"
														class="trash"
														slot="append"
														@click=${() => this.#requestRemoveTab(tab)}
														compact>
														<uui-icon name="umb:trash"></uui-icon>
												  </uui-button> `
												: nothing}
									  </div>`}
							</div>
						</uui-tab>`;
					},
				)}
			</uui-tab-group>
			<uui-button id="add-tab" @click="${this.#addTab}" label="Add tab" compact>
				<uui-icon name="umb:add"></uui-icon>
				Add tab
			</uui-button>`;
	}

	renderActions() {
		return html`<div class="tab-actions">
			<uui-button label="Compositions" compact>
				<uui-icon name="umb:merge"></uui-icon>
				Compositions
			</uui-button>
			<uui-button label="Reorder" compact>
				<uui-icon name="umb:navigation"></uui-icon>
				Reorder
			</uui-button>
		</div>`;
	}

	render() {
		return html`
			<umb-body-layout header-fit-height>
				<div id="header" slot="header">
					<div id="tabs-wrapper">${this._routerPath ? this.renderTabsNavigation() : ''}</div>
					${this.renderActions()}
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

			#tabs-wrapper {
				display: flex;
			}

			.content-tab-is-empty {
				align-self: center;
				border-radius: 3px;
				--uui-tab-text: var(--uui-color-text-alt);
				border: dashed 1px var(--uui-color-border-emphasis);
			}

			uui-tab {
				border-left: 1px solid transparent;
				border-right: 1px solid var(--uui-color-border);
			}

			uui-tab:not(:hover, :focus) .trash {
				opacity: 0;
				transition: opacity 120ms;
			}

			.tab {
				position: relative;
			}

			.external {
				vertical-align: sub;
			}

			uui-input:not(:focus, :hover) {
				border: 1px solid transparent;
			}

			.no-edit {
				display: inline-flex;
				padding-left: var(--uui-size-space-3);
				border: 1px solid transparent;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			.trash {
				opacity: 1;
				transition: opacity 120ms;
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
