import { UUITextStyles } from '@umbraco-ui/uui-css';
import { css, html } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { UmbLanguageStore, UmbLanguageStoreItemType, UMB_LANGUAGE_STORE_CONTEXT_TOKEN } from '../../language.store';
import { UmbTableColumn, UmbTableConfig, UmbTableItem } from '../../../../shared/components/table';
import { UmbWorkspaceEntityElement } from '../../../../shared/components/workspace/workspace-entity-element.interface';
import { UmbLitElement } from '@umbraco-cms/element';

import '../language/language-workspace.element';
import './language-root-table-delete-column-layout.element';
import './language-root-table-name-column-layout.element';

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends UmbLitElement implements UmbWorkspaceEntityElement {
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
				text-decoration: none;
			}
		`,
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
			elementName: 'umb-language-root-table-name-column-layout',
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

	private _languageStore?: UmbLanguageStore;

	constructor() {
		super();

		this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
			this._languageStore = instance;
			this._observeLanguages();
		});
	}

	load(): void {
		// Not relevant for this workspace
	}

	create(): void {
		// Not relevant for this workspace
	}

	private _observeLanguages() {
		this._languageStore?.getAll().subscribe((languages) => {
			this._createTableItems(languages);
		});
	}

	private _createTableItems(languages: Array<UmbLanguageStoreItemType>) {
		this._tableItems = languages.map((language) => {
			return {
				key: language.isoCode ?? '',
				icon: 'umb:globe',
				data: [
					{
						columnAlias: 'languageName',
						value: {
							name: language.name,
							key: language.isoCode,
						},
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
						value: languages.find((x) => x.isoCode === language.fallbackIsoCode)?.name,
					},
					{
						columnAlias: 'delete',
						value: language,
					},
				],
			};
		});
	}

	render() {
		return html`
			<umb-body-layout no-header-background>
				<a id="add-language" slot="header" href="section/settings/language/create/root">
					<uui-button label="Add language" look="outline" color="default"></uui-button>
				</a>
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
