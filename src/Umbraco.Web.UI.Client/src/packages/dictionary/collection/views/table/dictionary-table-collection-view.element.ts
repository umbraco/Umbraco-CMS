import type { UmbDictionaryCollectionModel } from '../../types.js';
import { UMB_EDIT_DICTIONARY_WORKSPACE_PATH_PATTERN } from '../../../workspace/paths.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
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

	#collectionContext?: UmbDefaultCollectionContext<UmbDictionaryCollectionModel>;
	#languageCollectionRepository = new UmbLanguageCollectionRepository(this);

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	async #observeCollectionItems() {
		if (!this.#collectionContext) return;

		const { data: languageData } = await this.#languageCollectionRepository.requestCollection({});
		if (!languageData) return;

		this.observe(
			this.#collectionContext.items,
			(collectionItems) => {
				this.#createTableColumns(languageData.items);
				this.#createTableItems(collectionItems, languageData.items);
			},
			'umbCollectionItemsObserver',
		);
	}

	#createTableColumns(languages: Array<UmbLanguageDetailModel>) {
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

	#createTableItems(dictionaries: Array<UmbDictionaryCollectionModel>, languages: Array<UmbLanguageDetailModel>) {
		this._tableItems = dictionaries.map((dictionary) => {
			const editPath = UMB_EDIT_DICTIONARY_WORKSPACE_PATH_PATTERN.generateAbsolute({
				unique: dictionary.unique,
			});

			return {
				id: dictionary.unique,
				icon: 'icon-book-alt-2',
				data: [
					{
						columnAlias: 'name',
						value: html`<a style="font-weight:bold" href=${editPath}> ${dictionary.name}</a> `,
					},
					...languages.map((language) => {
						const translation = dictionary.translations?.[language.unique];
						return {
							columnAlias: language.unique,
							value: translation ? this.#renderTranslation(translation) : this.#renderMissingTranslation(language.name),
						};
					}),
				],
			};
		});
	}

	#renderTranslation(value: string) {
		return html`<span style="color:var(--uui-color-text)" title="${value}">${value}</span>`;
	}

	#renderMissingTranslation(languageName: string) {
		return html`<span style="color:var(--uui-color-danger-standalone);font-style:italic" title="${this.localize.term('visuallyHiddenTexts_noTranslation')} (${languageName})">—</span>`;
	}

	override render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static override styles = [
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
