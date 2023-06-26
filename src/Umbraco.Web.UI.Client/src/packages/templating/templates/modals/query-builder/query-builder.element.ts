import { UmbTemplateRepository } from '../../repository/template.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/internal/modal';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import {
	TemplateQueryExecuteModel,
	TemplateQueryResultResponseModel,
	TemplateQuerySettingsResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

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

	#queryRequest: TemplateQueryExecuteModel = <TemplateQueryExecuteModel>{};

	@state()
	private _queryBuiilderSettings?: TemplateQuerySettingsResponseModel;

	#modalManagerContext?: UmbModalManagerContext;
	#templateRepository: UmbTemplateRepository;

	constructor() {
		super();
		this.#templateRepository = new UmbTemplateRepository(this);

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
		if (data) this._queryBuiilderSettings = data;
	}

	async #postTemplateQuery() {
		const { data, error } = await this.#templateRepository.postTemplateQueryExecute({
			requestBody: this.#queryRequest,
		});
		if (data) this._templateQuery = data;
	}

	render() {
		return html`
			<umb-body-layout headline="Query builder">
				<div id="main">
					<uui-box>
						<div>
							I want <uui-button look="outline">all content</uui-button> from
							<uui-button look="outline">all pages </uui-button>
						</div>
						<div>
							where <uui-button look="outline"></uui-button> from <uui-button look="outline">all pages </uui-button>
						</div>
					</uui-box>
				</div>

				<code> ${this._templateQuery?.queryExpression ?? ''} </code>
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
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-templating-query-builder-modal': UmbChooseInsertTypeModalElement;
	}
}
