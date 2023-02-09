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

			#main {
				margin: var(--uui-size-space-6);
			}
		`,
	];

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
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

	#languageStore?: UmbLanguageStore;

	constructor() {
		super();

		this.consumeContext(UMB_LANGUAGE_STORE_CONTEXT_TOKEN, (instance) => {
			this.#languageStore = instance;
			this.#observeLanguages();
		});
	}

	load(): void {
		// Not relevant for this workspace
	}

	create(): void {
		// Not relevant for this workspace
	}

	#observeLanguages() {
		this.#languageStore?.getAll().subscribe((languages) => {
			this.#createTableItems(languages);
		});
	}

	#createTableItems(languages: Array<UmbLanguageStoreItemType>) {
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
			<umb-body-layout headline="Languages">
				<div id="main">
					<div>
						<uui-button
							label="Add language"
							look="outline"
							color="default"
							href="section/settings/language/create/root"></uui-button>
					</div>
					<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
				</div>
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
