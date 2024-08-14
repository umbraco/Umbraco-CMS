import type { UmbTreeSelectionConfiguration } from '../types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './tree-picker-modal.token.js';
import { html, customElement, state, ifDefined, nothing, css, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UMB_WORKSPACE_MODAL, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';
import { debounce } from '@umbraco-cms/backoffice/utils';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbSearchProvider } from '@umbraco-cms/backoffice/search';
import { UmbTreePickerModalContext } from './tree-picker-modal.context.js';

@customElement('umb-tree-picker-modal')
export class UmbTreePickerModalElement<TreeItemType extends UmbTreeItemModelBase> extends UmbModalBaseElement<
	UmbTreePickerModalData<TreeItemType>,
	UmbTreePickerModalValue
> {
	@state()
	_selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	_createPath?: string;

	@state()
	_createLabel?: string;

	@state()
	private _searchIsEnabled = false;

	@state()
	private _searchQuery: string = '';

	@state()
	private _searchResult: Array<any> = [];

	@state()
	private _searching = false;

	#searchProvider?: any;

	// TODO: find a way to implement through the manifest api field
	#api = new UmbTreePickerModalContext(this);

	constructor() {
		super();
		this.observe(
			this.#api.selection.selection,
			(selection) => {
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbSelectionObserver',
		);
	}

	override connectedCallback() {
		super.connectedCallback();

		// TODO: We should make a nicer way to observe the value..  [NL]
		// This could be by observing when the modalCOntext gets set. [NL]
		this._selectionConfiguration.selection = this.value?.selection ?? [];
		this.#api.selection.setSelection(this.value?.selection ?? []);

		// Same here [NL]
		this._selectionConfiguration.multiple = this.data?.multiple ?? false;
		this.#api.selection.setMultiple(this.data?.multiple ?? false);
		this.#api.selection.setSelectable(true);

		this.#initCreateAction();
		this.#initSearch();
	}

	// Tree Selection

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#api.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#api.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	// Search

	async #initSearch() {
		const search = this.data?.search;

		if (!search) {
			this._searchIsEnabled = false;
			return;
		}

		this.#searchProvider = await createExtensionApiByAlias<UmbSearchProvider>(this, search.providerAlias);
		if (!this.#searchProvider) throw new Error(`Search Provider with alias ${search.providerAlias} is not available`);

		this._searchIsEnabled = true;
	}

	#onSearchInput(event: UUIInputEvent) {
		const value = event.target.value as string;
		this._searchQuery = value;

		if (!this._searchQuery) {
			this._searchResult = [];
			this._searching = false;
			return;
		}

		this._searching = true;
		this.#debouncedSearch();
	}

	#debouncedSearch = debounce(this.#search, 300);

	async #search() {
		if (!this._searchQuery) return;
		const { data } = await this.#searchProvider.search({ query: this._searchQuery });
		this._searchResult = data?.items ?? [];
		this._searching = false;
	}

	#onSearchClear() {
		this._searchQuery = '';
		this._searchResult = [];
	}

	// Create action
	#initCreateAction() {
		// TODO: If data.enableCreate is true, we should add a button to create a new item. [NL]
		// Does the tree know enough about this, for us to be able to create a new item? [NL]
		// I think we need to be able to get entityType and a parentId?, or do we only allow creation in the root? and then create via entity actions? [NL]
		// To remove the hardcoded URLs for workspaces of entity types, we could make an create event from the tree, which either this or the sidebar impl. will pick up and react to. [NL]
		// Or maybe the tree item context base can handle this? [NL]
		// Maybe its a general item context problem to be solved. [NL]
		const createAction = this.data?.createAction;
		if (createAction) {
			this._createLabel = createAction.label;
			new UmbModalRouteRegistrationController(
				this,
				(createAction.modalToken as typeof UMB_WORKSPACE_MODAL) ?? UMB_WORKSPACE_MODAL,
			)
				.onSetup(() => {
					return { data: createAction.modalData };
				})
				.onSubmit((value) => {
					if (value) {
						this.value = { selection: [value.unique] };
						this._submitModal();
					} else {
						this._rejectModal();
					}
				})
				.observeRouteBuilder((routeBuilder) => {
					const oldPath = this._createPath;
					this._createPath =
						routeBuilder({}) + createAction.extendWithPathPattern.generateLocal(createAction.extendWithPathParams);
					this.requestUpdate('_createPath', oldPath);
				});
		}
	}

	override render() {
		return html`
			<umb-body-layout headline="Select">
				<uui-box> ${this.#renderSearch()} ${this.#renderTree()}</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					${this._createPath
						? html` <uui-button
								label=${this.localize.string(this._createLabel ?? 'general_create')}
								look="secondary"
								href=${this._createPath}></uui-button>`
						: nothing}
					<uui-button
						label=${this.localize.term('general_choose')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderTree() {
		return html`
			<umb-tree
				alias=${ifDefined(this.data?.treeAlias)}
				.props=${{
					hideTreeItemActions: true,
					hideTreeRoot: this.data?.hideTreeRoot,
					selectionConfiguration: this._selectionConfiguration,
					filter: this.data?.filter,
					selectableFilter: this.data?.pickableFilter,
					startNode: this.data?.startNode,
					foldersOnly: this.data?.foldersOnly,
				}}
				@selected=${this.#onSelected}
				@deselected=${this.#onDeselected}></umb-tree>
		`;
	}

	#renderSearch() {
		if (!this._searchIsEnabled) {
			return nothing;
		}

		return html`
			<uui-input .value=${this._searchQuery} id="search-input" placeholder="Search..." @input=${this.#onSearchInput}>
				<div slot="prepend">
					${this._searching
						? html`<uui-loader-circle id="search-indicator"></uui-loader-circle>`
						: html`<uui-icon name="search"></uui-icon>`}
				</div>

				${this._searchQuery
					? html`
							<uui-button slot="append" type="button" @click=${this.#onSearchClear} compact>
								<uui-icon name="icon-delete" @click=${this.#onSearchClear}></uui-icon>
							</uui-button>
						`
					: nothing}
			</uui-input>
			<div id="search-divider"></div>
			${this.#renderSearchResult()}
		`;
	}

	#renderSearchResult() {
		if (this._searchQuery && this._searching === false && this._searchResult.length === 0) {
			return this.#renderEmptySearchResult();
		}

		return html`
			${repeat(
				this._searchResult,
				(item) => item.unique,
				(item) => this.#renderPickerSearchResultItem(item),
			)}
		`;
	}

	#renderEmptySearchResult() {
		return html`<small>No result for <strong>"${this._searchQuery}"</strong>.</small>`;
	}

	#renderPickerSearchResultItem(item: any) {
		return html`
			<umb-picker-search-result-item
				.props=${{ item }}
				.item=${item}
				.entityType=${item.entityType}></umb-picker-search-result-item>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#search-input {
				width: 100%;
			}

			#search-divider {
				width: 100%;
				height: 1px;
				background-color: var(--uui-color-divider);
				margin-top: var(--uui-size-space-5);
				margin-bottom: var(--uui-size-space-3);
			}

			#search-indicator {
				margin-left: 7px;
				margin-top: 4px;
			}
		`,
	];
}

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
