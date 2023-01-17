import { Language, LanguageResource } from '@umbraco-cms/backend-api';
import { UmbLitElement } from '@umbraco-cms/element';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html, LitElement } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import {
	UmbTableColumn,
	UmbTableConfig,
	UmbTableElement,
	UmbTableItem,
	UmbTableSelectedEvent,
} from 'src/backoffice/shared/components/table';

import '../language/language-workspace.element';
import './language-root-table-delete-column-layout.element';

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			umb-table {
				padding: 0;
				margin: var(--uui-size-space-3) var(--uui-size-space-6);
			}
			#add-language {
				margin-left: var(--uui-size-space-6);
			}
		`,
	];

	@state()
	private _languages: Array<Language> = [
		{
			id: 1,
			name: 'English',
			isoCode: 'en',
			isDefault: true,
			isMandatory: true,
		},
		{
			id: 2,
			name: 'Danish',
			isoCode: 'da',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
		{
			id: 3,
			name: 'German',
			isoCode: 'de',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
		{
			id: 4,
			name: 'French',
			isoCode: 'fr',
			isDefault: false,
			isMandatory: false,
			fallbackLanguageId: 1,
		},
	];

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: true,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Language',
			alias: 'languageName',
		},
		{
			name: 'ISO Code',
			alias: 'isoCode',
		},
		{
			name: 'Default language',
			alias: 'defaultLanguage',
		},
		{
			name: 'Mandatory language',
			alias: 'mandatoryLanguage',
		},
		{
			name: 'Fall back language',
			alias: 'fallBackLanguage',
		},
		{
			name: '',
			alias: 'delete',
			elementName: 'umb-language-root-table-delete-column-layout',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	constructor() {
		super();

		this._getLanguageData();
	}

	private async _getLanguageData() {
		const { data } = await tryExecuteAndNotify(this, LanguageResource.getLanguage({ skip: 0, take: 10 }));
		if (data) {
			this._languages = data.items;
			this._createTableItems(this._languages);
			console.log('LANGS', this._languages);
		}
	}

	private _createTableItems(languages: Array<Language>) {
		this._tableItems = languages.map((language) => {
			return {
				key: language.id?.toString() ?? '',
				icon: 'umb:globe',
				data: [
					{
						columnAlias: 'languageName',
						value: language.name,
					},
					{
						columnAlias: 'isoCode',
						value: language.isoCode,
					},
					{
						columnAlias: 'defaultLanguage',
						value: language.isDefault,
					},
					{
						columnAlias: 'mandatoryLanguage',
						value: language.isMandatory,
					},
					{
						columnAlias: 'fallBackLanguage',
						value: languages.find((x) => x.id === language.fallbackLanguageId)?.name,
					},
					{
						columnAlias: 'delete',
						value: {
							show: !language.isDefault,
						},
					},
				],
			};
		});
	}

	render() {
		// return html`<umb-language-workspace></umb-language-workspace>`;
		return html`
			<umb-body-layout no-header-background>
				<uui-button id="add-language" slot="header" label="Add language" look="outline" color="default"></uui-button>
				<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
			</umb-body-layout>
		`;
	}
}

export default UmbLanguageRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-workspace': UmbLanguageRootWorkspaceElement;
	}
}
