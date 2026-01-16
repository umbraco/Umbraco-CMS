import type { UmbCollectionSelectionConfiguration } from '../types.js';
import { UmbCollectionItemPickerContext } from './collection-item-picker-modal.context.js';
import type { UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue } from './types.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, nothing, ifDefined, css, classMap } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-collection-item-picker-modal')
export class UmbCollectionItemPickerModalElement extends UmbModalBaseElement<
	UmbCollectionItemPickerModalData,
	UmbCollectionItemPickerModalValue
> {
	@state()
	private _selectionConfiguration: UmbCollectionSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selectOnly: true,
		selection: [],
	};

	@state()
	private _hasSelection: boolean = false;

	@state()
	private _isSearchable: boolean = false;

	@state()
	private _searchQuery?: string;

	#pickerContext = new UmbCollectionItemPickerContext(this);

	constructor() {
		super();
		this.#pickerContext.selection.setSelectable(true);
		this.observe(this.#pickerContext.selection.hasSelection, (hasSelection) => {
			this._hasSelection = hasSelection;
		});
		this.#observePickerSelection();
		this.#observeSearch();
	}

	protected override async updated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>) {
		super.updated(_changedProperties);

		if (_changedProperties.has('data')) {
			if (this.data?.search) {
				this.#pickerContext.search.updateConfig({
					...this.data.search,
				});
			}

			const multiple = this.data?.multiple ?? false;
			this.#pickerContext.selection.setMultiple(multiple);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				multiple,
				selectableFilter: this.data?.pickableFilter,
			};
		}

		if (_changedProperties.has('value')) {
			const selection = this.value?.selection ?? [];
			this.#pickerContext.selection.setSelection(selection);

			this._selectionConfiguration = {
				...this._selectionConfiguration,
				selection: [...selection].filter((x) => x !== null),
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
			this.#pickerContext?.search.searchable,
			(isSearchable) => (this._isSearchable = isSearchable ?? false),
			null,
		);

		this.observe(
			this.#pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			null,
		);
	}

	#onItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.select(event.unique);
		this.modalContext?.dispatchEvent(new UmbSelectedEvent(event.unique));
	}

	#onItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.deselect(event.unique);
		this.modalContext?.dispatchEvent(new UmbDeselectedEvent(event.unique));
	}

	#searchSelectableFilter = () => true;

	override render() {
		const renderCollection = !!this.data?.collection.alias;

		return html`
			<umb-body-layout
				headline="${this.localize.term('general_choose')}"
				?main-no-padding=${renderCollection}
				class=${classMap({ 'has-search': this._isSearchable, 'is-searching': !!this._searchQuery })}>
				${this.#renderSearch()} ${this.#renderMain(renderCollection)} ${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderSearch() {
		if (!this._isSearchable) return nothing;

		const selectableFilter =
			this.data?.search?.pickableFilter ?? this.data?.pickableFilter ?? this.#searchSelectableFilter;

		return html`
			<div id="search-container">
				<umb-picker-search-field></umb-picker-search-field>
				<umb-picker-search-result .pickableFilter=${selectableFilter}></umb-picker-search-result>
			</div>
		`;
	}

	#renderMain(hasCollectionAlias: boolean) {
		return html` ${hasCollectionAlias ? this.#renderCollection() : this.#renderCollectionMenu()} `;
	}

	#renderCollection() {
		return html`
			<umb-collection
				alias=${ifDefined(this.data?.collection.alias)}
				.config=${{
					selectionConfiguration: this._selectionConfiguration,
					bulkActionConfiguration: { enabled: false },
				}}
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}></umb-collection>
		`;
	}

	#renderCollectionMenu() {
		return html`
			<uui-box id="collection-menu-box"
				><umb-collection-menu
					alias=${ifDefined(this.data?.collection?.menuAlias)}
					.props=${{
						selectionConfiguration: this._selectionConfiguration,
						filterArgs: this.data?.collection?.filterArgs,
						filter: this.data?.filter,
						selectableFilter: this.data?.pickableFilter,
					}}
					@selected=${this.#onItemSelected}
					@deselected=${this.#onItemDeselected}></umb-collection-menu
			></uui-box>
		`;
	}

	#renderActions() {
		return html`
			<div slot="actions">
				<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				<uui-button
					label=${this.localize.term('general_choose')}
					look="primary"
					color="positive"
					@click=${this._submitModal}
					?disabled=${!this._hasSelection}></uui-button>
			</div>
		`;
	}

	static override styles = [
		css`
			umb-collection {
				display: block;
				height: fit-content;
			}

			umb-body-layout[main-no-padding].has-search {
				#search-container {
					padding: var(--uui-size-layout-1);
					padding-bottom: 0;
				}

				umb-collection {
					margin-top: calc(-1 * var(--uui-size-4));
				}
			}

			umb-body-layout.is-searching {
				umb-collection,
				#collection-menu-box {
					display: none;
				}
			}
		`,
	];
}

export { UmbCollectionItemPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-item-picker-modal': UmbCollectionItemPickerModalElement;
	}
}
