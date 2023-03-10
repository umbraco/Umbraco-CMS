import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { when } from 'lit-html/directives/when.js';
import { UmbTableConfig, UmbTableColumn, UmbTableItem } from '../../../../backoffice/shared/components/table';
import { UmbDictionaryRepository } from '../../dictionary/repository/dictionary.repository';
import {
	UmbCreateDictionaryModalResult,
	UMB_CREATE_DICTIONARY_MODAL_TOKEN,
} from '../../dictionary/entity-actions/create/';
import { UmbLitElement } from '@umbraco-cms/element';
import { DictionaryOverviewModel, LanguageModel } from '@umbraco-cms/backend-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

@customElement('umb-dashboard-translation-dictionary')
export class UmbDashboardTranslationDictionaryElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			#dictionary-top-bar {
				margin-bottom: var(--uui-size-space-5);
				display: flex;
				justify-content: space-between;
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

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableItemsFiltered: Array<UmbTableItem> = [];

	#dictionaryItems: DictionaryOverviewModel[] = [];

	#repo!: UmbDictionaryRepository;

	#modalContext!: UmbModalContext;

	#tableItems: Array<UmbTableItem> = [];

	#tableColumns: Array<UmbTableColumn> = [];

	#languages: Array<LanguageModel> = [];

	constructor() {
		super();

		new UmbContextConsumerController(this, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
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
			// key is name to allow filtering on the displayed value
			const tableItem: UmbTableItem = {
				key: dictionary.name ?? '',
				icon: 'umb:book-alt',
				data: [
					{
						columnAlias: 'name',
						value: html`<a style="font-weight:bold" href="/section/translation/dictionary-item/edit/${dictionary.key}">
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
			? this.#tableItems.filter((t) => t.key.includes(e.target.value))
			: this.#tableItems;
	}

	async #create() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		const modalHandler = this.#modalContext?.open(UMB_CREATE_DICTIONARY_MODAL_TOKEN, { unique: null });

		// TODO: get type from modal result
		const { name } = await modalHandler.onSubmit();
		if (!name) return;

		const result = await this.#repo?.create({ $type: '', name, parentKey: null, translations: [], key: '' });

		// TODO => get location header to route to new item
		console.log(result);
	}

	render() {
		return html` <div id="dictionary-top-bar">
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
			)}`;
	}
}

export default UmbDashboardTranslationDictionaryElement;
declare global {
	interface HTMLElementTagNameMap {
		'umb-dashboard-translation-dictionary': UmbDashboardTranslationDictionaryElement;
	}
}
