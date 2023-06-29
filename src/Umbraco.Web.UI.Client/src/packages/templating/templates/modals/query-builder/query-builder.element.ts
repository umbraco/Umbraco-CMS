import { UmbTemplateRepository } from '../../repository/template.repository.js';
import { UUIComboboxListElement, UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import {
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
import { UmbButtonWithDropdownElement } from '@umbraco-cms/backoffice/components';
import './query-builder-filter.element.js';

export interface TemplateQueryBuilderModalData {
	hidePartialViews?: boolean;
}

export interface TemplateQueryBuilderModalResult {
	value: string;
}

@customElement('umb-templating-query-builder-modal')
export default class UmbChooseInsertTypeModalElement extends UmbModalBaseElement<
	TemplateQueryBuilderModalData,
	TemplateQueryBuilderModalResult
> {
	@query('#content-type-dropdown')
	private _contentTypeDropdown?: UmbButtonWithDropdownElement;

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
	}

	@state()
	private _queryBuilderSettings?: TemplateQuerySettingsResponseModel;

	@state()
	private _selectedRootContentName = 'all pages';

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

	async #postTemplateQuery() {
		const { data, error } = await this.#templateRepository.postTemplateQueryExecute({
			requestBody: this._queryRequest,
		});
		console.log(this._queryRequest);
		if (data) this._templateQuery = { ...data };
	}

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

	#setContentType(event: Event) {
		const target = event.target as UUIComboboxListElement;
		this.#updateQueryRequest({ contentTypeAlias: (target.value as string) ?? '' });
		this.#postTemplateQuery();
		this._contentTypeDropdown!.closePopover();
	}

	render() {
		return html`
			<umb-body-layout headline="Query builder">
				<div id="main">
					<uui-box>
						<div class="row">
							I want
							<umb-button-with-dropdown look="outline" id="content-type-dropdown"
								>${this._queryRequest?.contentTypeAlias ?? 'all content'}
								<uui-combobox-list slot="dropdown" @change=${this.#setContentType} id="content-type-list">
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
							<uui-button look="outline" @click=${this.#openDocumentPicker}
								>${this._selectedRootContentName}
							</uui-button>
						</div>
						<umb-query-builder-filter class="row" .settings=${this._queryBuilderSettings}></umb-query-builder-filter>
						<div class="row">
							ordered by <uui-button look="outline">(allowed properties)</uui-button>
							<uui-button look="outline">ascending/descending</uui-button>
						</div>
						<div class="row">N items returned, in 0 ms</div>
						<code> ${this._templateQuery?.queryExpression ?? ''} </code>
					</uui-box>
				</div>

				<div slot="actions">
					<uui-button @click=${this.#close} look="secondary">Close</uui-button>
					<uui-button @click=${this.#submit} look="primary" color="positive">Submit</uui-button>
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

			#content-type-list {
				width: 250%;
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-query-builder-modal': UmbChooseInsertTypeModalElement;
	}
}
