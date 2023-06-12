import { UmbDictionaryRepository } from '../../dictionary/repository/dictionary.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, customElement, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTableConfig, UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DictionaryOverviewResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CREATE_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-dashboard-translation-dictionary')
export class UmbDashboardTranslationDictionaryElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableItemsFiltered: Array<UmbTableItem> = [];

	#dictionaryItems: DictionaryOverviewResponseModel[] = [];

	#repo!: UmbDictionaryRepository;

	#modalContext!: UmbModalManagerContext;

	#tableItems: Array<UmbTableItem> = [];

	#tableColumns: Array<UmbTableColumn> = [];

	#languages: Array<LanguageResponseModel> = [];

	constructor() {
		super();

		new UmbContextConsumerController(this, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async connectedCallback() {
		super.connectedCallback();

		this.#repo = new UmbDictionaryRepository(this);
		this.#languages = await this.#repo.getLanguages();
		await this.#getDictionaryItems();
	}

	async #getDictionaryItems() {
		if (!this.#repo) return;

		const { data } = await this.#repo.list(0, 1000);
		this.#dictionaryItems = data?.items ?? [];
		this.#setTableColumns();
		this.#setTableItems();
	}

	/**
	 * We don't know how many translation items exist for each dictionary until the data arrives
	 * so can not generate the columns in advance.
	 * @returns
	 */
	#setTableColumns() {
		this.#tableColumns = [
			{
				name: 'Name',
				alias: 'name',
			},
		];

		this.#languages.forEach((l) => {
			if (!l.name) return;

			this.#tableColumns.push({
				name: l.name ?? '',
				alias: l.isoCode ?? '',
			});
		});
	}

	#setTableItems() {
		this.#tableItems = this.#dictionaryItems.map((dictionary) => {
			// id is set to name to allow filtering on the displayed value
			// TODO: Generate URL for editing the dictionary item
			const tableItem: UmbTableItem = {
				id: dictionary.name ?? '',
				icon: 'umb:book-alt',
				data: [
					{
						columnAlias: 'name',
						value: html`<a
							style="font-weight:bold"
							href="/section/translation/workspace/dictionary-item/edit/${dictionary.id}">
							${dictionary.name}</a
						> `,
					},
				],
			};

			this.#languages.forEach((l) => {
				if (!l.isoCode) return;

				tableItem.data.push({
					columnAlias: l.isoCode,
					value: dictionary.translatedIsoCodes?.includes(l.isoCode)
						? html`<uui-icon
								name="check"
								title="Translation exists for ${l.name}"
								style="color:var(--uui-color-positive-standalone);display:inline-block"></uui-icon>`
						: html`<uui-icon
								name="alert"
								title="Translation does not exist for ${l.name}"
								style="color:var(--uui-color-danger-standalone);display:inline-block"></uui-icon>`,
				});
			});

			return tableItem;
		});

		this._tableItemsFiltered = this.#tableItems;
	}

	#filter(e: { target: HTMLInputElement }) {
		this._tableItemsFiltered = e.target.value
			? this.#tableItems.filter((t) => t.id.includes(e.target.value))
			: this.#tableItems;
	}

	async #create() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;
		if (!this.#repo) return;

		const modalContext = this.#modalContext?.open(UMB_CREATE_DICTIONARY_MODAL, { unique: null });

		const { name, parentId } = await modalContext.onSubmit();
		if (!name || parentId === undefined) return;

		const { data } = await this.#repo.createScaffold(parentId);
		if (!data) return;

		// TODO: Temp url construction:
		history.pushState({}, '', `/section/translation/workspace/dictionary-item/edit/${data.id}`);
	}

	render() {
		return html`
			<umb-body-layout header-transparent>
				<div id="header" slot="header">
					<uui-button type="button" look="outline" label="Create dictionary item" @click=${this.#create}
						>Create dictionary item</uui-button
					>
					<uui-input
						@keyup="${this.#filter}"
						placeholder="Type to filter..."
						label="Type to filter dictionary"
						id="searchbar">
						<div slot="prepend">
							<uui-icon name="search" id="searchbar_icon"></uui-icon>
						</div>
					</uui-input>
				</div>
				${when(
					this._tableItemsFiltered.length,
					() => html` <umb-table
						.config=${this._tableConfig}
						.columns=${this.#tableColumns}
						.items=${this._tableItemsFiltered}></umb-table>`,
					() => html`<umb-empty-state>There were no dictionary items found.</umb-empty-state>`
				)}
			</umb-body-layout>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			#header {
				display: flex;
				justify-content: space-between;
				width: 100%;
			}

			umb-table {
				display: inline;
				padding: 0;
			}

			umb-empty-state {
				margin: auto;
				font-size: var(--uui-size-6);
			}
		`,
	];
}

export default UmbDashboardTranslationDictionaryElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-translation-dictionary': UmbDashboardTranslationDictionaryElement;
	}
}
