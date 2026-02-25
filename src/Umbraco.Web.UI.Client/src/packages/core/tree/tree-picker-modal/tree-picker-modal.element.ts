import { UmbTreeItemPickerContext } from '../tree-item-picker/index.js';
import type { UmbTreeElement } from '../tree.element.js';
import type { UmbTreeItemModelBase, UmbTreeSelectionConfiguration } from '../types.js';
import type { UmbTreePickerModalData, UmbTreePickerModalValue } from './types.js';
import { customElement, html, ifDefined, nothing, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPickerModalBaseElement } from '@umbraco-cms/backoffice/picker';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityExpansionModel, UmbExpansionChangeEvent } from '@umbraco-cms/backoffice/utils';

@customElement('umb-tree-picker-modal')
export class UmbTreePickerModalElement<TreeItemType extends UmbTreeItemModelBase> extends UmbPickerModalBaseElement<
	TreeItemType,
	UmbTreePickerModalData<TreeItemType>,
	UmbTreePickerModalValue
> {
	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	@state()
	private _hasSelection: boolean = false;

	@state()
	private _createPath?: string;

	@state()
	private _createLabel?: string;

	@state()
	private _searchQuery?: string;

	@state()
	private _treeExpansion: UmbEntityExpansionModel = [];

	protected _pickerContext = new UmbTreeItemPickerContext(this);

	constructor() {
		super();
		this._pickerContext.selection.setSelectable(true);
		this.observe(this._pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
		});
		this.#observePickerSelection();
		this.#observeSearch();
		this.#observeExpansion();
	}

	override connectedCallback(): void {
		super.connectedCallback();
		this.#initCreateAction();
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			if (this.data?.search) {
				this._pickerContext.search.updateConfig({
					...this.data.search,
					searchFrom: this.data.startNode,
					dataTypeUnique: this._pickerContext.dataType?.unique,
				});
			}

			const multiple = this.data?.multiple ?? false;
			this._pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
			};
		}

		if (_changedProperties.has('value')) {
			const selection = this.value?.selection ?? [];
			this._pickerContext.selection.setSelection(selection);
			this._selectionConfiguration = {
				...this._selectionConfiguration,
				selection: [...selection],
			};
		}
	}

	#observePickerSelection() {
		this.observe(
			this._pickerContext.selection.selection,
			(selection) => {
				this.updateValue({ selection });
				this.requestUpdate();
			},
			'umbPickerSelectionObserver',
		);
	}

	#observeSearch() {
		this.observe(
			this._pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			'umbPickerSearchQueryObserver',
		);
	}

	#observeExpansion() {
		this.observe(
			this._pickerContext.expansion.expansion,
			(value) => {
				this._treeExpansion = value;
			},
			'umbTreeItemPickerExpansionObserver',
		);
	}

	// Tree Selection
	#onTreeItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onTreeItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this._pickerContext.selection.deselect(event.unique);
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

	#onTreeItemExpansionChange(event: UmbExpansionChangeEvent) {
		const target = event.target as UmbTreeElement;
		const expansion = target.getExpansion();
		this._pickerContext.expansion.setExpansion(expansion);
	}

	#searchSelectableFilter = () => true;

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('general_choose')}>
				${this.#renderSearch()} ${this.#renderTree()} ${this.#renderActions()}
			</umb-body-layout>
		`;
	}
	#renderSearch() {
		const selectableFilter =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter ?? this.#searchSelectableFilter;

		return html`
			<umb-picker-search-field></umb-picker-search-field>
			<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
		`;
	}

	#renderTree() {
		if (this._searchQuery) {
			return nothing;
		}

		return html`
			<uui-box>
				<umb-tree
					alias=${ifDefined(this.data?.treeAlias)}
					.props=${{
						hideTreeItemActions: true,
						hideTreeRoot: this.data?.hideTreeRoot,
						expandTreeRoot: this.data?.expandTreeRoot,
						selectionConfiguration: this._selectionConfiguration,
						filter: this.data?.filter,
						selectableFilter: this.data?.pickableFilter,
						startNode: this.data?.startNode,
						foldersOnly: this.data?.foldersOnly,
						expansion: this._treeExpansion,
					}}
					@selected=${this.#onTreeItemSelected}
					@deselected=${this.#onTreeItemDeselected}
					@expansion-change=${this.#onTreeItemExpansionChange}></umb-tree
			></uui-box>
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
