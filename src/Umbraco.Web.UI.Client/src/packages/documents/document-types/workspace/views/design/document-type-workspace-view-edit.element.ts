import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import { UmbDocumentTypeDetailModel } from '../../../types.js';
import type { UmbDocumentTypeWorkspaceViewEditTabElement } from './document-type-workspace-view-edit-tab.element.js';
import { css, html, customElement, state, repeat, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUIInputElement, UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypeContainerStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { encodeFolderName, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	DocumentTypePropertyTypeContainerResponseModel,
	PropertyTypeContainerModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbRoute } from '@umbraco-cms/backoffice/router';
import { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbConfirmModalData } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';

const SORTER_CONFIG: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
	compareElementToModel: (element: HTMLElement, model: DocumentTypePropertyTypeContainerResponseModel) => {
		return element.getAttribute('data-umb-tabs-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: PropertyTypeContainerModelBaseModel) => {
		return container.querySelector(`[data-umb-tabs-id='` + modelEntry.id + `']`);
	},
	identifier: 'content-type-tabs-sorter',
	itemSelector: '[data-umb-tabs-id]',
	containerSelector: '#tabs-group',
	disabledItemSelector: '[inherited]',
	resolveVerticalDirection: () => {
		return false;
	},
};

@customElement('umb-document-type-workspace-view-edit')
export class UmbDocumentTypeWorkspaceViewEditElement extends UmbLitElement implements UmbWorkspaceViewElement {
	public sorter?: UmbSorterController<PropertyTypeContainerModelBaseModel>;

	config: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
		...SORTER_CONFIG,
		// TODO: Missing handlers to work properly: performItemMove and performItemRemove
		performItemInsert: async (args) => {
			if (!this._tabs) return false;
			const oldIndex = this._tabs.findIndex((tab) => tab.id! === args.item.id);
			if (args.newIndex === oldIndex) return true;

			let sortOrder = 0;
			//TODO the sortOrder set is not correct
			if (this._tabs.length > 0) {
				if (args.newIndex === 0) {
					sortOrder = (this._tabs[0].sortOrder ?? 0) - 1;
				} else {
					sortOrder = (this._tabs[Math.min(args.newIndex, this._tabs.length - 1)].sortOrder ?? 0) + 1;
				}

				if (sortOrder !== args.item.sortOrder) {
					await this._tabsStructureHelper.partialUpdateContainer(args.item.id!, { sortOrder });
				}
			}

			return true;
		},
	};

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
	private sortModeActive?: boolean;

	@state()
	private _buttonDisabled: boolean = false;

	private _workspaceContext?: UmbDocumentTypeWorkspaceContext;

	private _tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbDocumentTypeDetailModel>(this);

	private _modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT_TOKEN.TYPE;

	constructor() {
		super();
		this.sorter = new UmbSorterController(this, this.config);

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
			this.observe(
				this._workspaceContext.isSorting,
				(isSorting) => (this.sortModeActive = isSorting),
				'_observeIsSorting',
			);
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

	#changeMode() {
		this._workspaceContext?.setIsSorting(!this.sortModeActive);

		if (this.sortModeActive && this._tabs) {
			this.sorter?.setModel(this._tabs);
		} else {
			this.sorter?.setModel([]);
		}
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
		const modalData: UmbConfirmModalData = {
			headline: 'Delete tab',
			content: html`<umb-localize key="contentTypeEditor_confirmDeleteTabMessage" .args=${[tab?.name ?? tab?.id]}>
					Are you sure you want to delete the tab <strong>${tab?.name ?? tab?.id}</strong>
				</umb-localize>
				<div style="color:var(--uui-color-danger-emphasis)">
					<umb-localize key="contentTypeEditor_confirmDeleteTabNotice">
						This will delete all items that doesn't belong to a composition.
					</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		};

		// TODO: If this tab is composed of other tabs, then notify that it will only delete the local tab.

		const modalHandler = this._modalManagerContext?.open(UMB_CONFIRM_MODAL, { data: modalData });

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

		const changedName = this._workspaceContext?.structure.makeContainerNameUniqueForOwnerContentType(
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

	render() {
		return html`
			<umb-body-layout header-fit-height>
				<div id="header" slot="header">
					<div id="tabs-wrapper" class="flex">
						${this._routerPath ? this.renderTabsNavigation() : ''} ${this.renderAddButton()}
					</div>
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

	renderAddButton() {
		if (this.sortModeActive) return;
		return html`<uui-button id="add-tab" @click="${this.#addTab}" label="Add tab" compact>
			<uui-icon name="icon-add"></uui-icon>
			Add tab
		</uui-button>`;
	}

	renderActions() {
		const sortButtonText = this.sortModeActive
			? this.localize.term('general_reorderDone')
			: this.localize.term('general_reorder');

		return html`<div class="tab-actions">
			<uui-button look="outline" label=${this.localize.term('contentTypeEditor_compositions')} compact>
				<uui-icon name="icon-merge"></uui-icon>
				${this.localize.term('contentTypeEditor_compositions')}
			</uui-button>
			<uui-button look="outline" label=${sortButtonText} compact @click=${this.#changeMode}>
				<uui-icon name="icon-navigation"></uui-icon>
				${sortButtonText}
			</uui-button>
		</div>`;
	}

	renderTabsNavigation() {
		if (!this._tabs) return;

		return html`<div id="tabs-group" class="flex">
			<uui-tab-group>
				${this.renderRootTab()}
				${repeat(
					this._tabs,
					(tab) => tab.id! + tab.name,
					(tab) => this.renderTab(tab),
				)}
			</uui-tab-group>
		</div>`;
	}

	renderRootTab() {
		const rootTabPath = this._routerPath + '/root';
		const rootTabActive = rootTabPath === this._activePath;
		return html`<uui-tab
			class=${this._hasRootGroups || rootTabActive ? '' : 'content-tab-is-empty'}
			label=${this.localize.term('general_content')}
			.active=${rootTabActive}
			href=${rootTabPath}>
			${this.localize.term('general_content')}
		</uui-tab>`;
	}

	renderTab(tab: PropertyTypeContainerModelBaseModel) {
		const path = this._routerPath + '/tab/' + encodeFolderName(tab.name || '');
		const tabActive = path === this._activePath;
		const tabInherited = !this._tabsStructureHelper.isOwnerContainer(tab.id!);

		return html`<uui-tab
			label=${tab.name ?? 'unnamed'}
			.active=${tabActive}
			href=${path}
			data-umb-tabs-id=${ifDefined(tab.id)}>
			${this.renderTabInner(tab, tabActive, tabInherited)}
		</uui-tab>`;
	}

	renderTabInner(tab: PropertyTypeContainerModelBaseModel, tabActive: boolean, tabInherited: boolean) {
		if (this.sortModeActive) {
			return html`<div class="no-edit">
				${tabInherited
					? html`<uui-icon class="external" name="icon-merge"></uui-icon>${tab.name!}`
					: html`<uui-icon name="icon-navigation" class="drag-${tab.id}"> </uui-icon>${tab.name!}
							<uui-input
								label="sort order"
								type="number"
								value=${ifDefined(tab.sortOrder)}
								style="width:50px"
								@keypress=${(e: UUIInputEvent) => this.#changeOrderNumber(tab, e)}></uui-input>`}
			</div>`;
		}

		if (tabActive && !tabInherited) {
			return html`<div class="tab">
				<uui-input
					id="input"
					look="placeholder"
					placeholder="Unnamed"
					label=${tab.name!}
					value="${tab.name!}"
					auto-width
					@change=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
					@blur=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
					@input=${() => (this._buttonDisabled = true)}
					@focus=${(e: UUIInputEvent) => (e.target.value ? nothing : (this._buttonDisabled = true))}>
					${this.renderDeleteFor(tab)}
				</uui-input>
			</div>`;
		}

		if (tabInherited) {
			return html`<div class="no-edit"><uui-icon name="icon-merge"></uui-icon>${tab.name!}</div>`;
		} else {
			return html`<div class="no-edit">${tab.name!} ${this.renderDeleteFor(tab)}</div>`;
		}
	}

	#changeOrderNumber(tab: PropertyTypeContainerModelBaseModel, e: UUIInputEvent) {
		if (!e.target.value || !tab.id) return;
		const sortOrder = Number(e.target.value);
		this._tabsStructureHelper.partialUpdateContainer(tab.id, { sortOrder });
	}

	renderDeleteFor(tab: PropertyTypeContainerModelBaseModel) {
		return html`<uui-button
			label=${this.localize.term('actions_remove')}
			class="trash"
			slot="append"
			?disabled=${this._buttonDisabled}
			@click=${() => this.#requestRemoveTab(tab)}
			compact>
			<uui-icon name="icon-trash"></uui-icon>
		</uui-button>`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#buttons-wrapper {
				flex: 1;
				display: flex;
				align-items: center;
				justify-content: space-between;
				align-items: stretch;
			}

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

			.flex {
				display: flex;
			}
			uui-tab-group {
				flex-wrap: nowrap;
			}

			.content-tab-is-empty {
				align-self: center;
				border-radius: 3px;
				--uui-tab-text: var(--uui-color-text-alt);
				border: dashed 1px var(--uui-color-border-emphasis);
			}

			uui-tab {
				position: relative;
				border-left: 1px hidden transparent;
				border-right: 1px solid var(--uui-color-border);
			}

			.no-edit uui-input {
				pointer-events: auto;
			}

			.no-edit {
				pointer-events: none;
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

			uui-tab:not(:hover, :focus) .trash {
				opacity: 0;
				transition: opacity 120ms;
			}

			uui-input:not(:focus, :hover) {
				border: 1px solid transparent;
			}

			.inherited {
				vertical-align: sub;
			}

			.--umb-sorter-placeholder > * {
				visibility: hidden;
			}

			.--umb-sorter-placeholder::after {
				content: '';
				position: absolute;
				inset: 2px;
				border: 1px dashed var(--uui-color-divider-emphasis);
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
