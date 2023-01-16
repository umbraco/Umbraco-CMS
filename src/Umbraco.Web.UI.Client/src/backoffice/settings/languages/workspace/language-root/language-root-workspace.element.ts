import { Language } from '@umbraco-cms/backend-api';
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

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends LitElement {
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
	private _languages: Array<Language> = [];

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
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [];

	/**
	 *
	 */
	constructor() {
		super();

		this._createTableItems(this._languages);
	}

	private _createTableItems(languages: Array<Language>) {
		this._tableItems = languages.map((language) => {
			return {
				key: language.id,
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
						value: language.fallbackLanguageId,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-body-layout no-header-background>
				<uui-button id="add-language" slot="header" label="Add language" look="outline" color="default"></uui-button>
				<!-- <div slot="header" id="toolbar">
				</div> -->
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
