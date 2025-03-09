import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '../../content-type-workspace.context-token.js';
import type { UmbContentTypeModel, UmbPropertyTypeContainerModel } from '../../../types.js';
import {
	UmbContentTypeContainerStructureHelper,
	UmbContentTypeMoveRootGroupsIntoFirstTabHelper,
} from '../../../structure/index.js';
import { UMB_COMPOSITION_PICKER_MODAL } from '../../../modals/constants.js';
import type { UmbContentTypeDesignEditorTabElement } from './content-type-design-editor-tab.element.js';
import { UmbContentTypeDesignEditorContext } from './content-type-design-editor.context.js';
import { css, html, customElement, state, repeat, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputElement, UUIInputEvent, UUITabElement } from '@umbraco-cms/backoffice/external/uui';
import { encodeFolderName } from '@umbraco-cms/backoffice/router';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbRoute, UmbRouterSlotChangeEvent, UmbRouterSlotInitEvent } from '@umbraco-cms/backoffice/router';
import type {
	ManifestWorkspaceViewContentTypeDesignEditorKind,
	UmbWorkspaceViewElement,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbConfirmModalData } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, umbConfirmModal, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';

@customElement('umb-content-type-design-editor')
export class UmbContentTypeDesignEditorElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#sorter = new UmbSorterController<UmbPropertyTypeContainerModel, UUITabElement>(this, {
		getUniqueOfElement: (element) => element.getAttribute('data-umb-tab-id'),
		getUniqueOfModel: (tab) => tab.id,
		identifier: 'content-type-tabs-sorter',
		itemSelector: 'uui-tab',
		containerSelector: 'uui-tab-group',
		disabledItemSelector: ':not([sortable])',
		resolvePlacement: (args) => args.relatedRect.left + args.relatedRect.width * 0.5 > args.pointerX,
		onChange: ({ model }) => {
			this._tabs = model;
		},
		onEnd: ({ item }) => {
			/**
			 * Explanation: If the item is the first in list, we compare it to the item behind it to set a sortOrder.
			 * If it's not the first in list, we will compare to the item in before it, and check the following item to see if it caused overlapping sortOrder, then update
			 * the overlap if true, which may cause another overlap, so we loop through them till no more overlaps...
			 */
			const model = this._tabs ?? [];
			const newIndex = model.findIndex((entry) => entry.id === item.id);

			// Doesn't exist in model
			if (newIndex === -1) return;

			// As origin we set prev sort order to -1, so if no other then our item will become 0
			let prevSortOrder = -1;

			// If not first in list, then get the sortOrder of the item before.  [NL]
			if (newIndex > 0 && model.length > 0) {
				prevSortOrder = model[newIndex - 1].sortOrder;
			}

			// increase the prevSortOrder and use it for the moved item,
			this.#tabsStructureHelper.partialUpdateContainer(item.id, {
				sortOrder: ++prevSortOrder,
			});

			// Adjust everyone right after, until there is a gap between the sortOrders: [NL]
			let i = newIndex + 1;
			let entry: UmbPropertyTypeContainerModel | undefined;
			// As long as there is an item with the index & the sortOrder is less or equal to the prevSortOrder, we will update the sortOrder:
			while ((entry = model[i]) !== undefined && entry.sortOrder <= prevSortOrder) {
				// Increase the prevSortOrder and use it for the item:
				this.#tabsStructureHelper.partialUpdateContainer(entry.id, {
					sortOrder: ++prevSortOrder,
				});

				i++;
			}
		},
	});

	#workspaceContext?: (typeof UMB_CONTENT_TYPE_WORKSPACE_CONTEXT)['TYPE'];
	#designContext = new UmbContentTypeDesignEditorContext(this);
	#tabsStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);

	set manifest(value: ManifestWorkspaceViewContentTypeDesignEditorKind) {
		this._compositionRepositoryAlias = value.meta.compositionRepositoryAlias;
	}

	@state()
	private _compositionRepositoryAlias?: string;
	//private _hasRootProperties = false;

	@state()
	private _hasRootGroups = false;

	@state()
	private _routes: UmbRoute[] = [];

	@state()
	_tabs?: Array<UmbPropertyTypeContainerModel>;

	@state()
	private _routerPath?: string;

	@state()
	private _activePath = '';

	private _activeTabId?: string;

	@state()
	private _sortModeActive?: boolean;

	constructor() {
		super();

		this.#sorter.disable();

		this.observe(
			this.#designContext.isSorting,
			(isSorting) => {
				this._sortModeActive = isSorting;
				if (isSorting) {
					this.#sorter.enable();
				} else {
					this.#sorter.disable();
				}
			},
			'_observeIsSorting',
		);

		//TODO: We need to differentiate between local and composition tabs (and hybrids)

		this.#tabsStructureHelper.setContainerChildType('Tab');
		this.#tabsStructureHelper.setIsRoot(true);
		this.observe(this.#tabsStructureHelper.mergedContainers, (tabs) => {
			this._tabs = tabs;
			this.#sorter.setModel(tabs);
			this._createRoutes();
		});

		// _hasRootProperties can be gotten via _tabsStructureHelper.hasProperties. But we do not support root properties currently.

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this.#tabsStructureHelper.setStructureManager(workspaceContext.structure);

			this.#observeRootGroups();
		});
	}

	#toggleSortMode() {
		this.#designContext?.setIsSorting(!this._sortModeActive);
	}

	async #observeRootGroups() {
		if (!this.#workspaceContext) return;

		this.observe(
			await this.#workspaceContext.structure.hasRootContainers('Group'),
			(hasRootGroups) => {
				this._hasRootGroups = hasRootGroups;
				this._createRoutes();
			},
			'_observeGroups',
		);
	}

	private _createRoutes() {
		// TODO: How about storing a set of elements based on tab ids? to prevent re-initializing the element when renaming..[NL]
		if (!this.#workspaceContext || !this._tabs || this._hasRootGroups === undefined) return;
		const routes: UmbRoute[] = [];

		// We gather the activeTab name to check for rename, this is a bit hacky way to redirect the user without noticing the url changes to the new name [NL]
		let activeTabName: string | undefined = undefined;

		if (this._tabs.length > 0) {
			this._tabs?.forEach((tab) => {
				const tabName = tab.name && tab.name !== '' ? tab.name : '-';
				if (tab.id === this._activeTabId) {
					activeTabName = tabName;
				}
				routes.push({
					path: `tab/${encodeFolderName(tabName)}`,
					component: () => import('./content-type-design-editor-tab.element.js'),
					setup: (component) => {
						(component as UmbContentTypeDesignEditorTabElement).containerId = tab.id;
					},
				});
			});
		}

		if (this._hasRootGroups || this._tabs.length === 0) {
			routes.push({
				path: 'root',
				component: () => import('./content-type-design-editor-tab.element.js'),
				setup: (component) => {
					(component as UmbContentTypeDesignEditorTabElement).containerId = null;
				},
			});
			routes.push({
				path: '',
				redirectTo: 'root',
				guards: [() => this._activeTabId === undefined],
			});
		} else {
			routes.push({
				path: '',
				redirectTo: routes[0]?.path,
				guards: [() => this._activeTabId === undefined],
			});
		}

		if (routes.length !== 0) {
			routes.push({
				path: `**`,
				component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
				guards: [() => this._activeTabId === undefined],
			});
		}

		// If we have an active tab name, then we might have a active tab name re-name, then we will redirect to the new name if it has been changed: [NL]
		if (activeTabName !== undefined) {
			if (this._activePath && this._routerPath) {
				const oldPath = this._activePath.split(this._routerPath)[1];
				const newPath = '/tab/' + encodeFolderName(activeTabName);
				if (oldPath !== newPath) {
					// Lets cheat a bit and update the activePath already, in this way our input does not loose focus [NL]
					this._activePath = this._routerPath + newPath;
					// Update the current URL, so we are still on this specific tab: [NL]
					window.history.replaceState(null, '', this._activePath);
					// TODO: We have some flickering when renaming, this could potentially be fixed if we cache the view and re-use it if the same is requested [NL]
					// Or maybe its just about we just send the updated tabName to the view, and let it handle the update itself [NL]
				}
			}
		}

		routes.push({
			path: `**`,
			component: async () => (await import('@umbraco-cms/backoffice/router')).UmbRouteNotFoundElement,
		});

		this._routes = routes;
	}

	async #requestDeleteTab(tab: UmbPropertyTypeContainerModel | undefined) {
		if (!tab) return;
		// TODO: Localize this:
		const tabName = tab.name === '' ? 'Unnamed' : tab.name;
		// TODO: Localize this:
		const modalData: UmbConfirmModalData = {
			headline: 'Delete tab',
			content: html`<umb-localize key="contentTypeEditor_confirmDeleteTabMessage" .args=${[tabName]}>
					Are you sure you want to delete the tab <strong>${tabName}</strong>
				</umb-localize>
				<div style="color:var(--uui-color-danger-emphasis)">
					<umb-localize key="contentTypeEditor_confirmDeleteTabNotice">
						This will delete all items that doesn't belong to a composition.
					</umb-localize>
				</div>`,
			confirmLabel: this.localize.term('actions_delete'),
			color: 'danger',
		};

		// TODO: If this tab is composed of other tabs, then notify that it will only delete the local tab. [NL]

		await umbConfirmModal(this, modalData);

		this.#deleteTab(tab?.id);
	}
	#deleteTab(tabId?: string) {
		if (!tabId) return;
		this.#workspaceContext?.structure.removeContainer(null, tabId);
		if (this._activeTabId === tabId) {
			this._activeTabId = undefined;
		}
	}
	async #addTab() {
		// If there is already a Tab with no name, then focus it instead of adding a new one: [NL]
		// TODO: Optimize this so it looks at the data instead of the DOM [NL]
		const inputEl = this.shadowRoot?.querySelector('uui-tab[active] uui-input') as UUIInputElement;
		if (inputEl?.value === '') {
			this.#focusInput();
			return;
		}

		if (!this.#workspaceContext) {
			throw new Error('Workspace context has not been found');
		}

		if (!this._tabs) return;

		const len = this._tabs.length;
		const sortOrder = len === 0 ? 0 : this._tabs[len - 1].sortOrder + 1;
		const tab = await this.#workspaceContext.structure.createContainer(null, null, 'Tab', sortOrder);
		// If length was 0 before, then we need to move the root groups into the first tab: [NL]
		if (len === 0) {
			new UmbContentTypeMoveRootGroupsIntoFirstTabHelper(this, this.#workspaceContext.structure);
		}
		if (tab) {
			const path = this._routerPath + '/tab/' + encodeFolderName(tab.name && tab.name !== '' ? tab.name : '-');
			window.history.replaceState(null, '', path);
			this.#focusInput();
		}
	}

	async #focusInput() {
		setTimeout(() => {
			(this.shadowRoot?.querySelector('uui-tab[active] uui-input') as UUIInputElement | undefined)?.focus();
		}, 100);
	}

	async #tabNameChanged(event: InputEvent, tab: UmbPropertyTypeContainerModel) {
		this._activeTabId = tab.id;
		let newName = (event.target as HTMLInputElement).value;

		const changedName = this.#workspaceContext?.structure.makeContainerNameUniqueForOwnerContentType(
			tab.id,
			newName,
			'Tab',
		);

		// Check if it collides with another tab name of this same content-type, if so adjust name:
		// Notice changed name might be an empty string... [NL]
		if (changedName !== null && changedName !== undefined) {
			newName = changedName;
			(event.target as HTMLInputElement).value = newName;
		}

		this.#tabsStructureHelper.partialUpdateContainer(tab.id!, {
			name: newName,
		});
	}

	async #tabNameBlur(event: FocusEvent, tab: UmbPropertyTypeContainerModel) {
		if (!this._activeTabId) return;
		const newName = (event.target as HTMLInputElement | undefined)?.value;
		if (newName === '') {
			const changedName = this.#workspaceContext!.structure.makeEmptyContainerName(this._activeTabId, 'Tab');

			(event.target as HTMLInputElement).value = changedName;

			this.#tabsStructureHelper.partialUpdateContainer(tab.id!, {
				name: changedName,
			});
		}

		this._activeTabId = undefined;
	}

	async #openCompositionModal() {
		if (!this.#workspaceContext || !this._compositionRepositoryAlias) return;

		const unique = this.#workspaceContext.getUnique();
		if (!unique) {
			throw new Error('Content Type unique is undefined');
		}
		const contentTypes = this.#workspaceContext.structure.getContentTypes();
		const ownerContentType = this.#workspaceContext.structure.getOwnerContentType();
		if (!ownerContentType) {
			throw new Error('Owner Content Type not found');
		}

		const compositionConfiguration = {
			compositionRepositoryAlias: this._compositionRepositoryAlias,
			unique: unique,
			// Here we use the loaded content types to declare what we already inherit. That puts a pressure on cleaning up, but thats a good thing. [NL]
			selection: contentTypes.map((contentType) => contentType.unique).filter((id) => id !== unique),
			isElement: ownerContentType.isElement,
			currentPropertyAliases: [],
			isNew: this.#workspaceContext.getIsNew()!,
		};

		const value = await umbOpenModal(this, UMB_COMPOSITION_PICKER_MODAL, {
			data: compositionConfiguration,
		}).catch(() => undefined);

		if (!value) return;

		const compositionIds = value.selection;

		this.#workspaceContext?.setCompositions(
			compositionIds.map((unique) => ({ contentType: { unique }, compositionType: CompositionTypeModel.COMPOSITION })),
		);
	}

	override render() {
		return html`
			<umb-body-layout header-fit-height>
				<div id="header" slot="header">
					<div id="container-list">${this.renderTabsNavigation()} ${this.renderAddButton()}</div>
					${this.renderActions()}
				</div>
				<umb-router-slot
					.routes=${this._routes}
					@init=${(event: UmbRouterSlotInitEvent) => {
						this._routerPath = event.target.absoluteRouterPath;
					}}
					@change=${(event: UmbRouterSlotChangeEvent) => {
						this._activePath = event.target.absoluteActiveViewPath ?? '';
					}}>
				</umb-router-slot>
			</umb-body-layout>
		`;
	}

	renderAddButton() {
		// TODO: Localize this:
		if (this._sortModeActive) return;
		return html`
			<uui-button id="add-tab" @click="${this.#addTab}" label="Add tab">
				<uui-icon name="icon-add"></uui-icon>
				Add tab
			</uui-button>
		`;
	}

	renderActions() {
		const sortButtonText = this._sortModeActive
			? this.localize.term('general_reorderDone')
			: this.localize.term('general_reorder');

		return html`
			<div id="actions">
				${this._compositionRepositoryAlias
					? html`
							<uui-button
								look="outline"
								label=${this.localize.term('contentTypeEditor_compositions')}
								compact
								@click=${this.#openCompositionModal}>
								<uui-icon name="icon-merge"></uui-icon>
								${this.localize.term('contentTypeEditor_compositions')}
							</uui-button>
						`
					: ''}
				<uui-button look="outline" label=${sortButtonText} compact @click=${this.#toggleSortMode}>
					<uui-icon name="icon-navigation"></uui-icon>
					${sortButtonText}
				</uui-button>
			</div>
		`;
	}

	renderTabsNavigation() {
		if (!this._tabs || this._tabs.length === 0) return;

		return html`
			<div id="tabs-group">
				<uui-tab-group>
					${this.renderRootTab()}
					${repeat(
						this._tabs,
						(tab) => tab.id,
						(tab) => this.renderTab(tab),
					)}
				</uui-tab-group>
			</div>
		`;
	}

	renderRootTab() {
		const rootTabPath = this._routerPath + '/root';
		const rootTabActive = rootTabPath === this._activePath;
		if (!this._hasRootGroups && !this._sortModeActive) {
			// If we don't have any root groups and we are not in sort mode, then we don't want to render the root tab.
			return nothing;
		}
		return html`
			<uui-tab
				id="root-tab"
				class=${this._hasRootGroups || rootTabActive ? '' : 'content-tab-is-empty'}
				label=${this.localize.term('general_generic')}
				.active=${rootTabActive}
				href=${rootTabPath}>
				${this.localize.term('general_generic')}
			</uui-tab>
		`;
	}

	renderTab(tab: UmbPropertyTypeContainerModel) {
		const path = this._routerPath + '/tab/' + encodeFolderName(tab.name && tab.name !== '' ? tab.name : '-');
		const tabActive = path === this._activePath;
		const ownedTab = this.#tabsStructureHelper.isOwnerChildContainer(tab.id!) ?? false;

		return html`<uui-tab
			label=${tab.name && tab.name !== '' ? tab.name : 'Unnamed'}
			.active=${tabActive}
			href=${path}
			data-umb-tab-id=${ifDefined(tab.id)}
			?sortable=${ownedTab}>
			${this.renderTabInner(tab, tabActive, ownedTab)}
		</uui-tab>`;
	}

	renderTabInner(tab: UmbPropertyTypeContainerModel, tabActive: boolean, ownedTab: boolean) {
		// TODO: Localize this:
		const hasTabName = tab.name && tab.name !== '';
		const tabName = hasTabName ? tab.name : 'Unnamed';
		if (this._sortModeActive) {
			return html`<div class="tab">
				${ownedTab
					? html`<uui-icon name="icon-navigation" class="drag-${tab.id}"> </uui-icon>${tabName}
							<uui-input
								label="sort order"
								type="number"
								value=${ifDefined(tab.sortOrder)}
								style="width:50px"
								@change=${(e: UUIInputEvent) => this.#changeOrderNumber(tab, e)}></uui-input>`
					: html`<uui-icon name="icon-merge"></uui-icon>${tab.name!}`}
			</div>`;
		}

		if (tabActive && ownedTab) {
			return html`<div class="tab">
				<uui-input
					id="input"
					look="placeholder"
					placeholder="Unnamed"
					label=${tab.name!}
					value="${tab.name!}"
					auto-width
					minlength="1"
					@change=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
					@input=${(e: InputEvent) => this.#tabNameChanged(e, tab)}
					@blur=${(e: FocusEvent) => this.#tabNameBlur(e, tab)}>
					${this.renderDeleteFor(tab)}
				</uui-input>
			</div>`;
		}

		if (ownedTab) {
			return html`<div class="not-active">
				<span class=${hasTabName ? '' : 'invaild'}>${hasTabName ? tab.name : 'Unnamed'}</span> ${this.renderDeleteFor(
					tab,
				)}
			</div>`;
		} else {
			return html`<div class="not-active"><uui-icon name="icon-merge"></uui-icon>${tab.name!}</div>`;
		}
	}

	#changeOrderNumber(tab: UmbPropertyTypeContainerModel, e: UUIInputEvent) {
		if (!e.target.value || !tab.id) return;
		const sortOrder = Number(e.target.value);
		this.#tabsStructureHelper.partialUpdateContainer(tab.id, { sortOrder });
	}

	renderDeleteFor(tab: UmbPropertyTypeContainerModel) {
		return html`<uui-button
			label=${this.localize.term('actions_remove')}
			class="trash"
			slot="append"
			@click=${(e: MouseEvent) => {
				e.stopPropagation();
				e.preventDefault();
				this.#requestDeleteTab(tab);
			}}
			compact>
			<uui-icon name="icon-trash"></uui-icon>
		</uui-button>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				position: relative;
				display: flex;
				flex-direction: column;
				height: 100%;
				--uui-tab-background: var(--uui-color-surface);
			}

			#buttons-wrapper {
				flex: 1;
				display: flex;
				align-items: center;
				justify-content: space-between;
				align-items: stretch;
			}

			[drag-placeholder] {
				opacity: 0.5;
			}

			[drag-placeholder] uui-input {
				visibility: hidden;
			}

			/* TODO: This should be replaced with a general workspace bar — naming is hard */

			#header {
				width: 100%;
				min-height: var(--uui-size-16);
				display: flex;
				align-items: center;
				justify-content: space-between;
				flex-wrap: nowrap;
			}

			#container-list {
				display: flex;
			}

			#tabs-group {
				display: flex;
			}

			#actions {
				display: flex;
				gap: var(--uui-size-space-2);
			}

			uui-tab-group {
				flex-wrap: nowrap;
			}

			uui-tab.content-tab-is-empty {
				align-self: center;
				border-radius: 3px;
				--uui-tab-text: var(--uui-color-text-alt);
				border: dashed 1px var(--uui-color-border-emphasis);
			}

			uui-tab {
				position: relative;
				border-left: 1px hidden transparent;
				border-right: 1px solid var(--uui-color-border);
				background-color: var(--uui-color-surface);
			}

			.not-active uui-button {
				pointer-events: auto;
			}

			.not-active {
				pointer-events: none;
				display: inline-flex;
				padding-left: var(--uui-size-space-3);
				border: 1px solid transparent;
				align-items: center;
				gap: var(--uui-size-space-3);
			}

			.invaild {
				color: var(--uui-color-danger, #d42054);
			}

			.trash {
				opacity: 1;
				transition: opacity 100ms;
			}

			uui-tab:not(:hover, :focus) .trash {
				opacity: 0;
				transition: opacity 100ms;
			}

			uui-input:not(:focus, :hover, :invalid) {
				border: 1px solid transparent;
			}

			.inherited {
				vertical-align: sub;
			}

			[drag-placeholder] {
				opacity: 0.2;
			}
		`,
	];
}

export default UmbContentTypeDesignEditorElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-design-editor': UmbContentTypeDesignEditorElement;
	}
}
