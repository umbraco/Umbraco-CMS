import { UmbTemplateRepository } from '../../repository/template.repository.js';
import { UUIComboboxListElement, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, query, queryAll } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import {
	UMB_DOCUMENT_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import {
	TemplateQueryExecuteFilterPresentationModel,
	TemplateQueryExecuteModel,
	TemplateQueryExecuteSortModel,
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';
import { UmbButtonWithDropdownElement } from '@umbraco-cms/backoffice/components';
import './query-builder-filter.element.js';
import UmbQueryBuilderFilterElement from './query-builder-filter.element.js';

export interface TemplateQueryBuilderModalData {
	hidePartialViews?: boolean;
}

export interface TemplateQueryBuilderModalResult {
	value: string;
}

enum SortOrder {
	Ascending = 'ascending',
	Descending = 'descending',
}

@customElement('umb-templating-query-builder-modal')
export default class UmbChooseInsertTypeModalElement extends UmbModalBaseElement<
	TemplateQueryBuilderModalData,
	TemplateQueryBuilderModalResult
> {
	@query('#content-type-dropdown')
	private _contentTypeDropdown?: UmbButtonWithDropdownElement;

	@query('#sort-dropdown')
	private _sortDropdown?: UmbButtonWithDropdownElement;

	@query('#filter-container')
	private _filterContainer?: HTMLElement;

	@queryAll('umb-query-builder-filter')
	private _filterElements!: UmbQueryBuilderFilterElement[];

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		this.modalContext?.submit({
			value: this._templateQuery?.queryExpression ?? '',
		});
	}

	@state()
	private _templateQuery?: TemplateQueryResultResponseModel;

	@state()
	private _queryRequest: TemplateQueryExecuteModel = <TemplateQueryExecuteModel>{};

	#updateQueryRequest(update: TemplateQueryExecuteModel) {
		this._queryRequest = { ...this._queryRequest, ...update };
		this.#postTemplateQuery();
	}

	@state()
	private _queryBuilderSettings?: TemplateQuerySettingsResponseModel;

	@state()
	private _selectedRootContentName = 'all pages';

	@state()
	private _defaultSortDirection: SortOrder = SortOrder.Ascending;

	#documentRepository: UmbDocumentRepository;
	#modalManagerContext?: UmbModalManagerContext;
	#templateRepository: UmbTemplateRepository;

	constructor() {
		super();
		this.#templateRepository = new UmbTemplateRepository(this);
		this.#documentRepository = new UmbDocumentRepository(this);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
		this.#init();
	}

	#init() {
		this.#getTemplateQuerySettings();
		this.#postTemplateQuery();
	}

	async #getTemplateQuerySettings() {
		const { data, error } = await this.#templateRepository.getTemplateQuerySettings();
		if (data) this._queryBuilderSettings = data;
	}

	#postTemplateQuery = async () => {
		const { data, error } = await this.#templateRepository.postTemplateQueryExecute({
			requestBody: this._queryRequest,
		});
		console.log(this._queryRequest);
		if (data) this._templateQuery = { ...data };
	};

	#openDocumentPicker = () => {
		this.#modalManagerContext
			?.open(UMB_DOCUMENT_PICKER_MODAL)
			.onSubmit()
			.then((result) => {
				this.#updateQueryRequest({ rootContentId: result.selection[0] });

				if (result.selection.length > 0 && result.selection[0] === null) {
					this._selectedRootContentName = 'all pages';
					return;
				}

				if (result.selection.length > 0) {
					this.#getDocumentItem(result.selection as string[]);
					return;
				}
			});
	};

	async #getDocumentItem(ids: string[]) {
		const { data, error } = await this.#documentRepository.requestItemsLegacy(ids);
		if (data) {
			this._selectedRootContentName = data[0].name;
		}
	}

	#createFilterElement() {
		const filterElement = document.createElement('umb-query-builder-filter');
		filterElement.settings = this._queryBuilderSettings;
		filterElement.classList.add('row');
		filterElement.addEventListener('add-filter', this.#addFilterElement);
		filterElement.addEventListener('remove-filter', this.#removeFilter);
		filterElement.addEventListener('update-query', this.#updateFilters);
		return filterElement;
	}

	#setContentType(event: Event) {
		const target = event.target as UUIComboboxListElement;
		this.#updateQueryRequest({ contentTypeAlias: (target.value as string) ?? '' });
		this._contentTypeDropdown!.closePopover();
	}

	#setSortProperty(event: Event) {
		const target = event.target as UUIComboboxListElement;
		this.#updateQueryRequest({
			sort: { ...this._queryRequest.sort, propertyAlias: (target.value as string) ?? '' },
		});
		this._sortDropdown!.closePopover();
	}

	#setSortDirection() {
		if (!this._queryRequest.sort?.direction) {
			this.#updateQueryRequest({
				sort: { ...this._queryRequest.sort, direction: this._defaultSortDirection },
			});
			return;
		}

		this.#updateQueryRequest({
			sort: {
				...this._queryRequest.sort,
				direction:
					this._queryRequest.sort?.direction === SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending,
			},
		});
	}

	#addFilterElement = () => {
		this._filterContainer?.appendChild(this.#createFilterElement());
	};

	#updateFilters = () => {
		this.#updateQueryRequest({ filters: Array.from(this._filterElements)?.map((filter) => filter.filter) ?? [] });
	};

	#removeFilter = (event: Event) => {
		const target = event.target as UmbQueryBuilderFilterElement;
		this._filterContainer?.removeChild(target);
		this.#updateFilters();
	};

	render() {
		return html`
			<umb-body-layout headline="Query builder">
				<div id="main">
					<uui-box>
						<div class="row">
							I want
							<umb-button-with-dropdown look="outline" id="content-type-dropdown" label="Choose content type"
								>${this._queryRequest?.contentTypeAlias ?? 'all content'}
								<uui-combobox-list slot="dropdown" @change=${this.#setContentType} class="options-list">
									<uui-combobox-list-option value="">all content</uui-combobox-list-option>
									${this._queryBuilderSettings?.contentTypeAliases?.map(
										(alias) =>
											html`<uui-combobox-list-option .value=${alias}
												>content of type "${alias}"</uui-combobox-list-option
											>`
									)}
								</uui-combobox-list></umb-button-with-dropdown
							>
							from
							<uui-button look="outline" @click=${this.#openDocumentPicker} label="Choose root content"
								>${this._selectedRootContentName}
							</uui-button>
						</div>
						<div id="filter-container">
							<umb-query-builder-filter
								unremovable
								class="row"
								.settings=${this._queryBuilderSettings}
								@add-filter=${this.#addFilterElement}
								@update-query=${this.#updateFilters}
								@remove-filter=${this.#removeFilter}></umb-query-builder-filter>
						</div>
						<div class="row">
							ordered by
							<umb-button-with-dropdown look="outline" id="sort-dropdown" label="Property alias"
								>${this._queryRequest.sort?.propertyAlias ?? ''}
								<uui-combobox-list slot="dropdown" @change=${this.#setSortProperty} class="options-list">
									${this._queryBuilderSettings?.properties?.map(
										(property) =>
											html`<uui-combobox-list-option .value=${property.alias ?? ''}
												>${property.alias}</uui-combobox-list-option
											>`
									)}
								</uui-combobox-list></umb-button-with-dropdown
							>

							${this._queryRequest.sort?.propertyAlias
								? html`<uui-button look="outline" @click=${this.#setSortDirection}
										>${this._queryRequest.sort.direction ?? this._defaultSortDirection}</uui-button
								  >`
								: ''}
						</div>
						<div class="row">
							<span id="results-count"
								>${this._templateQuery?.resultCount ?? 0} items returned, in ${this._templateQuery?.executionTime ?? 0}
								ms</span
							>
						</div>
						<umb-code-block language="C#" copy> ${this._templateQuery?.queryExpression ?? ''} </umb-code-block>
					</uui-box>
				</div>

				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary" label="Close">Close</uui-button>
					<uui-button @click=${this.#submit} look="primary" color="positive" label="Submit">Submit</uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				color: var(--uui-color-text);
				--umb-header-layout-height: 70px;
			}

			#main {
				box-sizing: border-box;
				height: calc(
					100dvh - var(--umb-header-layout-height) - var(--umb-footer-layout-height) - 2 * var(--uui-size-layout-1)
				);
			}

			.options-list {
				min-width: 30ch;
				background-color: var(--uui-color-surface);
				box-shadow: var(--uui-shadow-depth-3);
			}

			uui-combobox-list-option {
				padding: 8px 20px;
			}

			.row {
				display: flex;
				gap: 10px;
				border-bottom: 1px solid #f3f3f5;
				align-items: center;
				padding: 20px 0;
			}

			#filter-container {
				flex-direction: column;
				justify-content: flex-start;
			}

			#results-count {
				font-weight: bold;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-query-builder-modal': UmbChooseInsertTypeModalElement;
	}
}
