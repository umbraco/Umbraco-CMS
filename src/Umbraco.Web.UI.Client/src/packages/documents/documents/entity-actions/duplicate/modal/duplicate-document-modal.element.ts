import { UMB_DOCUMENT_TREE_ALIAS } from '../../../tree/manifests.js';
import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../../../search/constants.js';
import type { UmbDuplicateDocumentModalData, UmbDuplicateDocumentModalValue } from './duplicate-document-modal.token.js';
import { html, customElement, nothing, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbPickerContext } from '@umbraco-cms/backoffice/picker';
import type { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeSelectionConfiguration } from '@umbraco-cms/backoffice/tree';
import type { UUIBooleanInputEvent } from '@umbraco-cms/backoffice/external/uui';
import type { UmbDocumentTreeItemModel } from '../../../types.js';

const elementName = 'umb-document-duplicate-to-modal';

@customElement(elementName)
export class UmbDocumentDuplicateToModalElement extends UmbModalBaseElement<
	UmbDuplicateDocumentModalData,
	UmbDuplicateDocumentModalValue
> {
	@state()
	private _destinationUnique?: string | null;

	@state()
	private _searchQuery?: string;

	@state()
	private _selectionConfiguration: UmbTreeSelectionConfiguration = {
		multiple: false,
		selectable: true,
		selection: [],
	};

	// Base picker context: provides UMB_PICKER_CONTEXT for the search field/result and a shared
	// selection manager so picking from the tree or from search results both feed one selection.
	#pickerContext = new UmbPickerContext(this);

	constructor() {
		super();
		this.#pickerContext.selection.setSelectable(true);
		this.#pickerContext.selection.setMultiple(false);
		this.#pickerContext.search.updateConfig({ providerAlias: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS });

		this.observe(
			this.#pickerContext.selection.selection,
			(selection) => {
				this._destinationUnique = selection.length ? selection[0] : undefined;
				this._selectionConfiguration = { ...this._selectionConfiguration, selection: [...selection] };
				if (this._destinationUnique || this._destinationUnique === null) {
					this.updateValue({ destination: { unique: this._destinationUnique } });
				}
			},
			'umbPickerSelectionObserver',
		);

		this.observe(
			this.#pickerContext.search.query,
			(query) => {
				this._searchQuery = query?.query;
			},
			'umbPickerSearchQueryObserver',
		);
	}

	#selectableFilter = (item: UmbDocumentTreeItemModel): boolean => {
		if (!this.data?.selectableFilter) return true;
		return this.data.selectableFilter(item);
	};

	#onTreeItemSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.select(event.unique);
	}

	#onTreeItemDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		this.#pickerContext.selection.deselect(event.unique);
	}

	#onRelateToOriginalChange(event: UUIBooleanInputEvent) {
		this.updateValue({ relateToOriginal: event.target.checked });
	}

	#onIncludeDescendantsChange(event: UUIBooleanInputEvent) {
		this.updateValue({ includeDescendants: event.target.checked });
	}

	override render() {
		if (!this.data) return nothing;

		return html`
			<umb-body-layout headline=${this.localize.term('actions_copyTo')}>
				<uui-box id="tree-box">
					<umb-picker-search-field></umb-picker-search-field>
					<umb-picker-search-result .pickableFilter=${this.#selectableFilter}></umb-picker-search-result>
					${this.#renderTree()}
				</uui-box>

				<uui-box headline=${this.localize.term('general_options')}>
					<umb-property-layout
						label=${this.localize.term('defaultdialogs_relateToOriginalLabel')}
						orientation="vertical">
						<div slot="editor">
							<uui-toggle
								label=${this.localize.term('defaultdialogs_relateToOriginalLabel')}
								@change=${this.#onRelateToOriginalChange}
								.checked=${this.value?.relateToOriginal ?? false}></uui-toggle>
						</div>
					</umb-property-layout>

					<umb-property-layout label=${this.localize.term('defaultdialogs_includeDescendants')} orientation="vertical">
						<div slot="editor">
							<uui-toggle
								label=${this.localize.term('defaultdialogs_includeDescendants')}
								@change=${this.#onIncludeDescendantsChange}
								.checked=${this.value?.includeDescendants ?? false}></uui-toggle>
						</div>
					</umb-property-layout>
				</uui-box>

				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderTree() {
		if (this._searchQuery) return nothing;

		return html`
			<umb-tree
				alias=${UMB_DOCUMENT_TREE_ALIAS}
				.props=${{
					hideTreeItemActions: true,
					expandTreeRoot: true,
					selectionConfiguration: this._selectionConfiguration,
					selectableFilter: this.#selectableFilter,
					expansion: this.data?.treeExpansion ?? [],
				}}
				@selected=${this.#onTreeItemSelected}
				@deselected=${this.#onTreeItemDeselected}></umb-tree>
		`;
	}

	#renderActions() {
		return html`
			<uui-button
				slot="actions"
				label=${this.localize.term('general_cancel')}
				@click="${this._rejectModal}"></uui-button>
			<uui-button
				slot="actions"
				color="positive"
				look="primary"
				label=${this.localize.term('general_copy')}
				@click=${this._submitModal}
				?disabled=${this._destinationUnique === undefined}></uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#tree-box {
				margin-bottom: var(--uui-size-layout-1);
			}
		`,
	];
}

export { UmbDocumentDuplicateToModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentDuplicateToModalElement;
	}
}
