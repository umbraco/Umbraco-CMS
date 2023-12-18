import { UmbTemplateRepository } from '../../repository/template.repository.js';
import { localizePropertyType, localizeSort } from './utils.js';
import type { UmbQueryBuilderFilterElement } from './query-builder-filter.element.js';
import { UUIComboboxListElement } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, query, queryAll, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbModalBaseElement,
	UMB_DOCUMENT_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
} from '@umbraco-cms/backoffice/modal';
import {
	TemplateQueryExecuteModel,
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbDocumentRepository } from '@umbraco-cms/backoffice/document';
import './query-builder-filter.element.js';

export interface TemplateQueryBuilderModalData {
	hidePartialViews?: boolean;
}

export interface UmbTemplateQueryBuilderModalValue {
	value: string;
}

enum SortOrder {
	Ascending = 'ascending',
	Descending = 'descending',
}

@customElement('umb-templating-query-builder-modal')
export default class UmbChooseInsertTypeModalElement extends UmbModalBaseElement<
	TemplateQueryBuilderModalData,
	UmbTemplateQueryBuilderModalValue
> {
	@query('#filter-container')
	private _filterContainer?: HTMLElement;

	@queryAll('umb-query-builder-filter')
	private _filterElements!: UmbQueryBuilderFilterElement[];

	@state()
	private _templateQuery?: TemplateQueryResultResponseModel;

	@state()
	private _queryRequest: TemplateQueryExecuteModel = <TemplateQueryExecuteModel>{};

	@state()
	private _queryBuilderSettings?: TemplateQuerySettingsResponseModel;

	@state()
	private _selectedRootContentName? = this.localize.term('template_websiteRoot');

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

	#close() {
		this.modalContext?.reject();
	}

	#submit() {
		this.modalContext?.submit();
	}

	#updateQueryRequest(update: TemplateQueryExecuteModel) {
		this._queryRequest = { ...this._queryRequest, ...update };
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
		if (data) {
			this._templateQuery = { ...data };
			this.value = { value: this._templateQuery?.queryExpression ?? '' };
		}
	};

	#openDocumentPicker = () => {
		this.#modalManagerContext
			?.open(UMB_DOCUMENT_PICKER_MODAL, { data: { hideTreeRoot: true } })
			.onSubmit()
			.then((result) => {
				// eslint-disable-next-line @typescript-eslint/ban-ts-comment
				// @ts-ignore
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
		const { data, error } = await this.#documentRepository.requestItems(ids);
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
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.#updateQueryRequest({ contentTypeAlias: (target.value as string) ?? '' });
	}

	#setSortProperty(event: Event) {
		const target = event.target as UUIComboboxListElement;

		if (!this._queryRequest.sort) this.#setSortDirection();

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.#updateQueryRequest({
			sort: { ...this._queryRequest.sort, propertyAlias: (target.value as string) ?? '' },
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

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
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
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this.#updateQueryRequest({ filters: Array.from(this._filterElements)?.map((filter) => filter.filter) ?? [] });
	};

	#removeFilter = (event: Event) => {
		const target = event.target as UmbQueryBuilderFilterElement;
		this._filterContainer?.removeChild(target);
		this.#updateFilters();
	};

	render() {
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
									${this._queryRequest?.contentTypeAlias ?? this.localize.term('template_allContent')}
								</span>
								<uui-combobox-list @change=${this.#setContentType} class="options-list">
									<uui-combobox-list-option value="">all content</uui-combobox-list-option>
									${this._queryBuilderSettings?.contentTypeAliases?.map(
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
							<uui-button look="outline" @click=${this.#openDocumentPicker} label="Choose root content">
								${this._selectedRootContentName}
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
						<div class="row">
							<span id="results-count">
								${this._templateQuery?.resultCount ?? 0}
								<umb-localize key="template_itemsReturned">items returned, in</umb-localize>
								${this._templateQuery?.executionTime ?? 0} ms
							</span>
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

	static styles = [
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-query-builder-modal': UmbChooseInsertTypeModalElement;
	}
}
