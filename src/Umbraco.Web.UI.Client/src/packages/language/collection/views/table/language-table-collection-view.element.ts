import type { UmbLanguageDetailModel } from '../../../types.js';
import type { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UMB_COLLECTION_CONTEXT } from '@umbraco-cms/backoffice/collection';
import type { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import { css, html, customElement, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

import './column-layouts/name/language-table-name-column-layout.element.js';

@customElement('umb-language-table-collection-view')
export class UmbLanguageTableCollectionViewElement extends UmbLitElement {
	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Name',
			alias: 'languageName',
			elementName: 'umb-language-table-name-column-layout',
		},
		{
			name: 'ISO Code',
			alias: 'isoCode',
		},
		{
			name: 'Default',
			alias: 'defaultLanguage',
		},
		{
			name: 'Mandatory',
			alias: 'mandatoryLanguage',
		},
		{
			name: 'Fallback',
			alias: 'fallbackLanguage',
		},
		{
			name: '',
			alias: 'entityActions',
			align: 'right',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];
	private _cultureNames = new Intl.DisplayNames('en', { type: 'language' });

	#collectionContext?: UmbDefaultCollectionContext<UmbLanguageDetailModel>;

	constructor() {
		super();

		this.consumeContext(UMB_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeCollectionItems();
		});
	}

	#observeCollectionItems() {
		if (!this.#collectionContext) return;
		this.observe(this.#collectionContext.items, (items) => this.#createTableItems(items), 'umbCollectionItemsObserver');
	}

	#createTableItems(languages: Array<UmbLanguageDetailModel>) {
		this._tableItems = languages.map((language) => {
			return {
				id: language.unique,
				icon: 'icon-globe',
				data: [
					{
						columnAlias: 'languageName',
						value: {
							name: language.name ? language.name : this._cultureNames.of(language.unique),
							unique: language.unique,
						},
					},
					{
						columnAlias: 'isoCode',
						value: language.unique,
					},
					{
						columnAlias: 'defaultLanguage',
						value: html`<umb-boolean-table-column-view .value=${language.isDefault}></umb-boolean-table-column-view>`,
					},
					{
						columnAlias: 'mandatoryLanguage',
						value: html`<umb-boolean-table-column-view .value=${language.isMandatory}></umb-boolean-table-column-view>`,
					},
					{
						columnAlias: 'fallbackLanguage',
						value: languages.find((x) => x.unique === language.fallbackIsoCode)?.name,
					},
					{
						columnAlias: 'entityActions',
						value: html`<umb-entity-actions-table-column-view
							.value=${{
								entityType: language.entityType,
								unique: language.unique,
							}}></umb-entity-actions-table-column-view>`,
					},
				],
			};
		});
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

export default UmbLanguageTableCollectionViewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-table-collection-view': UmbLanguageTableCollectionViewElement;
	}
}
