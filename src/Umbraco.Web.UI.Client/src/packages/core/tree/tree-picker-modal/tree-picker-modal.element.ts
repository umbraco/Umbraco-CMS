import type { UmbTreeSelectionConfiguration } from '../types.js';
import { UmbTreeItemPickerContext } from '../tree-item-picker/index.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './tree-picker-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';

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
	_hasSelection: boolean = false;

	@state()
	_createPath?: string;

	@state()
	_createLabel?: string;

	@state()
	_searchQuery?: string;

	#pickerContext = new UmbTreeItemPickerContext(this);

	constructor() {
		super();
		this.#pickerContext.selection.setSelectable(true);
		this.observe(this.#pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
		});
		this.#observePickerSelection();
		this.#observeSearch();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#initCreateAction();
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			if (this.data?.search) {
				this.#pickerContext.search.updateConfig({
					...this.data.search,
					searchFrom: this.data.startNode,
				});

				const searchQueryParams = this.data.search.queryParams;
				if (searchQueryParams) {
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					//@ts-ignore - TODO wire up types
					this.#pickerContext.search.setQuery(searchQueryParams);
				}
			}

			const multiple = this.data?.multiple ?? false;
			this.#pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
			};
		}

		if (_changedProperties.has('value')) {
			const selection = this.value?.selection ?? [];
			this.#pickerContext.selection.setSelection(selection);
			this._selectionConfiguration = {
				...this._selectionConfiguration,
				selection: [...selection],
			};
		}
	}

	#observePickerSelection() {
		this.observe(
			this.#pickerContext.selection.selection,
			(selection) => {
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbPickerSelectionObserver',
		);
	}

	#observeSearch() {
		this.observe(
			this.#pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			'umbPickerSearchQueryObserver',
		);
	}

	// Tree Selection
	#onTreeItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onTreeItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
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
			<umb-body-layout headline=${this.localize.term('general_choose')}>
				<uui-box> ${this.#renderSearch()} ${this.#renderTree()}</uui-box>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}
	#renderSearch() {
		return html`
			<umb-picker-search-field></umb-picker-search-field>
			<umb-picker-search-result></umb-picker-search-result>
		`;
	}

	#renderTree() {
		if (this._searchQuery) {
			return nothing;
		}

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
				@selected=${this.#onTreeItemSelected}
				@deselected=${this.#onTreeItemDeselected}></umb-tree>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				${this._createPath
					? html` <uui-button
							label=${this.localize.string(this._createLabel ?? '#general_create')}
							look="secondary"
							href=${this._createPath}></uui-button>`
					: nothing}
				<uui-button
					label=${this.localize.term('general_choose')}
					look="primary"
					color="positive"
					@click=${this._submitModal}
					?disabled=${!this._hasSelection}></uui-button>
			</div>
		`;
	}
}

export default UmbTreePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-picker-modal': UmbTreePickerModalElement<UmbTreeItemModelBase>;
	}
}
