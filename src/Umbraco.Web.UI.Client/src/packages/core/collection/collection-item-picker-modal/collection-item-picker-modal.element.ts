import type { UmbCollectionSelectionConfiguration } from '../types.js';
import { UmbCollectionItemPickerContext } from './collection-item-picker-modal.context.js';
import type { UmbCollectionItemPickerModalData, UmbCollectionItemPickerModalValue } from './types.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, nothing, ifDefined } from '@umbraco-cms/backoffice/external/lit';
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
		return html`
			${this.data?.collection.alias
				? html` <umb-body-layout headline=${this.localize.term('general_choose')} main-no-padding>
						${this.#renderCollection()} ${this.#renderActions()}
					</umb-body-layout>`
				: html` <umb-body-layout headline=${this.localize.term('general_choose')}>
						<uui-box> ${this.#renderSearch()} ${this.#renderCollectionMenu()}</uui-box>
						${this.#renderActions()}
					</umb-body-layout>`}
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

	#renderCollection() {
		return html`
			<umb-collection
				alias=${this.data?.collection.alias}
				.config=${{
					selectionConfiguration: this._selectionConfiguration,
				}}
				@selected=${this.#onItemSelected}
				@deselected=${this.#onItemDeselected}></umb-collection>
		`;
	}

	#renderCollectionMenu() {
		if (this._searchQuery) {
			return nothing;
		}

		return html` <umb-collection-menu
			alias=${ifDefined(this.data?.collection?.menuAlias)}
			.props=${{
				selectionConfiguration: this._selectionConfiguration,
				filterArgs: this.data?.collection?.filterArgs,
				filter: this.data?.filter,
				selectableFilter: this.data?.pickableFilter,
			}}
			@selected=${this.#onItemSelected}
			@deselected=${this.#onItemDeselected}></umb-collection-menu>`;
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
}

export { UmbCollectionItemPickerModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-item-picker-modal': UmbCollectionItemPickerModalElement;
	}
}
