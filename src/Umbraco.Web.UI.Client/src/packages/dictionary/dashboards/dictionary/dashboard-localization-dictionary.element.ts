import { UmbDictionaryRepository } from '../../dictionary/repository/dictionary.repository.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, html, customElement, state, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbTableConfig, UmbTableColumn, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { DictionaryOverviewResponseModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';

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

	#tableItems: Array<UmbTableItem> = [];

	#tableColumns: Array<UmbTableColumn> = [];

	#languages: Array<LanguageResponseModel> = [];

	constructor() {
		super();
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
				name: this.localize.term('general_name'),
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
				data: [
					{
						columnAlias: 'name',
						value: html`<a
							style="font-weight:bold"
							href="/section/dictionary/workspace/dictionary-item/edit/${dictionary.id}">
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
								title="${this.localize.term('visuallyHiddenTexts_hasTranslation')} (${l.name})"
								style="color:var(--uui-color-positive-standalone);display:inline-block"></uui-icon>`
						: html`<uui-icon
								name="alert"
								title="${this.localize.term('visuallyHiddenTexts_noTranslation')} (${l.name})"
								style="color:var(--uui-color-danger-standalone);display:inline-block"></uui-icon>`,
				});
			});

			return tableItem;
		});

		this._tableItemsFiltered = this.#tableItems;
	}

	#filter(e: { target: HTMLInputElement }) {
		const searchValue = e.target.value.toLocaleLowerCase();
		this._tableItemsFiltered = searchValue
			? this.#tableItems.filter((t) => t.id.toLocaleLowerCase().includes(searchValue))
			: this.#tableItems;
	}

	render() {
		return html`
			<umb-body-layout header-transparent>
				<div id="header" slot="header">
					<uui-button
						type="button"
						look="outline"
						label=${this.localize.term('dictionary_createNew')}
						href="section/dictionary/workspace/dictionary-item/create/null">
						${this.localize.term('dictionary_createNew')}
					</uui-button>
					<uui-input
						@keyup="${this.#filter}"
						placeholder=${this.localize.term('placeholders_filter')}
						label=${this.localize.term('placeholders_filter')}
						id="searchbar">
						<div slot="prepend">
							<uui-icon name="search" id="searchbar_icon"></uui-icon>
						</div>
					</uui-input>
				</div>
				${when(
					this._tableItemsFiltered.length,
					() =>
						html` <umb-table
							.config=${this._tableConfig}
							.columns=${this.#tableColumns}
							.items=${this._tableItemsFiltered}></umb-table>`,
					() => html`<umb-empty-state>${this.localize.term('emptyStates_emptyDictionaryTree')}</umb-empty-state>`,
				)}
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
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
