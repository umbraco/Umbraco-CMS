import type { UmbExecuteTemplateQueryRequestModel } from '../../repository/query/types.js';
import { UmbTemplateQueryRepository } from '../../repository/query/index.js';
import { localizePropertyType, localizeSort } from './utils.js';
import type { UmbTemplateQueryBuilderFilterElement } from './query-builder-filter.element.js';
import type {
	UmbTemplateQueryBuilderModalData,
	UmbTemplateQueryBuilderModalValue,
} from './query-builder-modal.token.js';
import type { UUIComboboxListElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, query, queryAll, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement, UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type {
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDocumentItemRepository, UMB_DOCUMENT_PICKER_MODAL } from '@umbraco-cms/backoffice/document';
import './query-builder-filter.element.js';

enum SortOrder {
	Ascending = 'ascending',
	Descending = 'descending',
}

@customElement('umb-template-query-builder-modal')
export default class UmbTemplateQueryBuilderModalElement extends UmbModalBaseElement<
	UmbTemplateQueryBuilderModalData,
	UmbTemplateQueryBuilderModalValue
> {
	@query('#filter-container')
	private _filterContainer?: HTMLElement;

	@queryAll('umb-template-query-builder-filter')
	private _filterElements!: UmbTemplateQueryBuilderFilterElement[];

	@state()
	private _templateQuery?: TemplateQueryResultResponseModel;

	@state()
	private _queryRequest: UmbExecuteTemplateQueryRequestModel = <UmbExecuteTemplateQueryRequestModel>{};

	@state()
	private _queryBuilderSettings?: TemplateQuerySettingsResponseModel;

	@state()
	private _selectedRootContentName? = this.localize.term('template_websiteRoot');

	@state()
	private _defaultSortDirection: SortOrder = SortOrder.Ascending;

	#documentItemRepository: UmbDocumentItemRepository;
	#templateQueryRepository: UmbTemplateQueryRepository;

	constructor() {
		super();
		this.#templateQueryRepository = new UmbTemplateQueryRepository(this);
		this.#documentItemRepository = new UmbDocumentItemRepository(this);

		this.#init();
	}

	#init() {
		this.#getTemplateQuerySettings();
		this.#executeTemplateQuery();
	}

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		this.modalContext?.submit();
	}

	#updateQueryRequest(update: Partial<UmbExecuteTemplateQueryRequestModel>) {
		this._queryRequest = { ...this._queryRequest, ...update };
		this.#executeTemplateQuery();
	}

	async #getTemplateQuerySettings() {
		const { data } = await this.#templateQueryRepository.requestTemplateQuerySettings();
		if (data) this._queryBuilderSettings = data;
	}

	#executeTemplateQuery = async () => {
		const { data } = await this.#templateQueryRepository.executeTemplateQuery(this._queryRequest);
		if (data) {
			this._templateQuery = { ...data };
			this.value = { value: this._templateQuery?.queryExpression ?? '' };
		}
	};

	async #openDocumentPicker() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		modalManager
			.open(this, UMB_DOCUMENT_PICKER_MODAL, { data: { hideTreeRoot: true } })
			.onSubmit()
			.then((result) => {
				const selection = result.selection[0];
				this.#updateQueryRequest({ rootDocument: selection ? { unique: selection } : null });

				if (result.selection.length > 0 && result.selection[0] === null) {
					this._selectedRootContentName = 'all pages';
					return;
				}

				if (result.selection.length > 0) {
					this.#getDocumentItem(result.selection as string[]);
					return;
				}
			});
	}

	async #getDocumentItem(ids: string[]) {
		const { data } = await this.#documentItemRepository.requestItems(ids);
		if (data) {
			// TODO: get correct variant name
			this._selectedRootContentName = data[0].variants[0].name;
		}
	}

	#createFilterElement() {
		const filterElement = document.createElement('umb-template-query-builder-filter');
		filterElement.settings = this._queryBuilderSettings;
		filterElement.classList.add('row');
		filterElement.addEventListener('add-filter', this.#addFilterElement);
		filterElement.addEventListener('remove-filter', this.#removeFilter);
		filterElement.addEventListener('update-query', this.#updateFilters);
		return filterElement;
	}

	#setContentType(event: Event) {
		const target = event.target as UUIComboboxListElement;
		this.#updateQueryRequest({ documentTypeAlias: target.value as string });
	}

	#setSortProperty(event: Event) {
		const target = event.target as UUIComboboxListElement;

		if (!this._queryRequest.sort) this.#setSortDirection();

		this.#updateQueryRequest({
			sort: { ...this._queryRequest.sort, propertyAlias: target.value as string },
		});
	}

	#setSortDirection() {
		if (!this._queryRequest.sort?.direction) {
			this.#updateQueryRequest({
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
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
		// Only use the filter from elements that have everything set
		const ready = Array.from(this._filterElements)?.filter((element) => element.isFilterValid);
		this.#updateQueryRequest({ filters: ready?.map((element) => element.filter) ?? [] });
	};

	#removeFilter = (event: Event) => {
		if (this._filterElements.length > 1) {
			const target = event.target as UmbTemplateQueryBuilderFilterElement;
			this._filterContainer?.removeChild(target);
			if (this._filterElements.length === 1) {
				this._filterElements[0].unremovable = true;
			}
		}
		this.#updateFilters();
	};

	override render() {
		const properties = localizePropertyType(this._queryBuilderSettings?.properties);
		const sort = localizeSort(this._queryRequest.sort);
		return html`
			<umb-body-layout headline=${this.localize.term('template_queryBuilder')}>
				<div id="main">
					<uui-box>
						<div class="row">
							<umb-localize key="template_iWant">I want</umb-localize>
							<umb-dropdown look="outline" id="content-type-dropdown" label="Choose content type">
								<span slot="label">
									${this._queryRequest?.documentTypeAlias ?? this.localize.term('template_allContent')}
								</span>
								<uui-combobox-list @change=${this.#setContentType} class="options-list">
									<uui-combobox-list-option value="">all content</uui-combobox-list-option>
									${this._queryBuilderSettings?.documentTypeAliases?.map(
										(alias) =>
											html`<uui-combobox-list-option .value=${alias}>
												<umb-localize key="template_contentOfType" .args=${[alias]}>
													content of type "${alias}"
												</umb-localize>
											</uui-combobox-list-option>`,
									)}
								</uui-combobox-list></umb-dropdown
							>
							<umb-localize key="template_from">from</umb-localize>
							<uui-button look="outline" @click=${this.#openDocumentPicker} label="Choose root document">
								${this._selectedRootContentName}
							</uui-button>
						</div>
						<div id="filter-container">
							<umb-template-query-builder-filter
								unremovable
								class="row"
								.settings=${this._queryBuilderSettings}
								@add-filter=${this.#addFilterElement}
								@update-query=${this.#updateFilters}
								@remove-filter=${this.#removeFilter}></umb-template-query-builder-filter>
						</div>
						<div class="row">
							<umb-localize key="template_orderBy">order by</umb-localize>
							<umb-dropdown look="outline" id="sort-dropdown" label="Property alias">
								<span slot="label">${this._queryRequest.sort?.propertyAlias ?? ''}</span>
								<uui-combobox-list @change=${this.#setSortProperty} class="options-list">
									${properties?.map(
										(property) =>
											html`<uui-combobox-list-option .value=${property.alias ?? ''}>
												<umb-localize key=${ifDefined(property.localizeKey)}>${property.alias}</umb-localize>
											</uui-combobox-list-option>`,
									)}
								</uui-combobox-list>
							</umb-dropdown>

							${sort?.propertyAlias
								? html`<uui-button look="outline" @click=${this.#setSortDirection}>
										<umb-localize key=${ifDefined(sort.localizeKey)}>
											${sort.direction ?? this._defaultSortDirection}
										</umb-localize>
									</uui-button>`
								: ''}
						</div>
						<div class="row query-results">
							<span id="results-count">
								${this._templateQuery?.resultCount ?? 0}
								<umb-localize key="template_itemsReturned">items returned, in</umb-localize>
								${this._templateQuery?.executionTime ?? 0} ms
							</span>
							${this._templateQuery?.sampleResults.map(
								(sample) => html`<span><umb-icon name=${sample.icon}></umb-icon>${sample.name}</span>`,
							) ?? ''}
						</div>
						<umb-code-block language="C#" copy>${this._templateQuery?.queryExpression ?? ''}</umb-code-block>
					</uui-box>
				</div>

				<div slot="actions">
					<uui-button
						@click=${this.#close}
						look="secondary"
						label=${this.localize.term('buttons_confirmActionCancel')}></uui-button>
					<uui-button
						@click=${this.#submit}
						look="primary"
						color="positive"
						label=${this.localize.term('buttons_submitChanges')}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
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

			uui-combobox-list-option {
				padding: 8px 20px;
				margin: 0;
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
			.query-results {
				flex-direction: column;
				align-items: flex-start;
				gap: 0;
			}
			.query-results span {
				display: flex;
				align-items: center;
				gap: var(--uui-size-1);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-template-query-builder-modal': UmbTemplateQueryBuilderModalElement;
	}
}
