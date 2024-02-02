import type { UmbDictionaryDetailModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_DEFAULT_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';

@customElement('umb-dictionary-table-collection-view')
export class UmbDictionaryTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'name',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	#collectionContext?: UmbDefaultCollectionContext<UmbDictionaryDetailModel>;
	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_DEFAULT_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	async firstUpdated() {
		const { data } = await this.#languageCollectionRepository.requestCollection({ skip: 0, take: 1000 });
		if (!data) return;
		this.#createTableColumns(data.items);
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableColumns(languages) {
		const columns: Array<UmbTableColumn> = [
			{
				name: this.localize.term('general_name'),
				alias: 'name',
			},
		];

		const languageColumns: Array<UmbTableColumn> = languages.map((language) => {
			return {
				name: language.name,
				alias: language.unique,
			};
		});

		this._tableColumns = [...columns, ...languageColumns];
	}

	#createTableItems(dictionaries: Array<UmbDictionaryDetailModel>) {
		this._tableItems = dictionaries.map((dictionary) => {
			return {
				id: dictionary.unique,
				icon: 'icon-globe',
				data: [
					{
						columnAlias: 'name',
						value: html`<a
							style="font-weight:bold"
							href="section/dictionary/workspace/dictionary/edit/${dictionary.unique}">
							${dictionary.name}</a
						> `,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			:host {
				display: flex;
				flex-direction: column;
			}
		`,
	];
}

export default UmbDictionaryTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-dictionary-table-collection-view': UmbDictionaryTableCollectionViewElement;
	}
}
