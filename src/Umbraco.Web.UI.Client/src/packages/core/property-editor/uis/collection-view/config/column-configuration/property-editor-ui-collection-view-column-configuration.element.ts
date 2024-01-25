import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property, repeat, css, query, state } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputEvent, UUISelectElement } from '@umbraco-cms/backoffice/external/uui';
import type {
	UmbPropertyEditorConfigCollection} from '@umbraco-cms/backoffice/property-editor';
import {
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

interface ColumnConfig {
	alias: string;
	header: string;
	isSystem: boolean;
	nameTemplate?: string;
}

/**
 * @element umb-property-editor-ui-collection-view-column-configuration
 */
@customElement('umb-property-editor-ui-collection-view-column-configuration')
export class UmbPropertyEditorUICollectionViewColumnConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	value: Array<ColumnConfig> = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	private _options: Array<Option> = [
		{ value: 'sortOrder', name: this.localize.term('general_sort'), group: 'System Fields' },
		{ value: 'updateDate', name: this.localize.term('content_updateDate'), group: 'System Fields' },
		{ value: 'updater', name: this.localize.term('content_updatedBy'), group: 'System Fields' },
		{ value: 'createDate', name: this.localize.term('content_createDate'), group: 'System Fields' },
		{ value: 'owner', name: this.localize.term('content_createBy'), group: 'System Fields' },
		{ value: 'published', name: this.localize.term('content_isPublished'), group: 'System Fields' },
		{ value: 'contentTypeAlias', name: this.localize.term('content_documentType'), group: 'System Fields' },
		{ value: 'email', name: this.localize.term('general_email'), group: 'System Fields' },
		{ value: 'username', name: this.localize.term('general_username'), group: 'System Fields' },
		/*
		{ value: 'contentTreePicker', name: 'contentTreePicker', group: 'Custom Fields' },
		{ value: 'ete', name: 'ete', group: 'Custom Fields' },
		{ value: 'link', name: 'link', group: 'Custom Fields' },
		{ value: 'listViewContent', name: 'listViewContent', group: 'Custom Fields' },
		{ value: 'mediaTreepicker', name: 'mediaTreepicker', group: 'Custom Fields' },
		{ value: 'memberTreepicker', name: 'memberTreepicker', group: 'Custom Fields' },
		{ value: 'multinode', name: 'multinode', group: 'Custom Fields' },
		{ value: 'richtext', name: 'richtext', group: 'Custom Fields' },
		{ value: 'text', name: 'text', group: 'Custom Fields' },
		{ value: 'umbracoBytes', name: 'umbracoBytes', group: 'Custom Fields' },
		{ value: 'umbracoExtension', name: 'umbracoExtension', group: 'Custom Fields' },
		{ value: 'umbracoFile', name: 'umbracoFile', group: 'Custom Fields' },
		{ value: 'umbracoHeight', name: 'umbracoHeight', group: 'Custom Fields' },
		{ value: 'umbracoMemberComments', name: 'umbracoMemberComments', group: 'Custom Fields' },
		{ value: 'umbracoWidth', name: 'umbracoWidth', group: 'Custom Fields' },
		{ value: 'uploadAFile', name: 'uploadAFile', group: 'Custom Fields' },
		{ value: 'uploader', name: 'uploader', group: 'Custom Fields' },
		*/
	];

	@query('uui-select')
	private _selectEl!: UUISelectElement;

	#onAdd() {
		const selected = this._options.find((config) => config.value === this._selectEl.value);
		if (!selected) return;

		const duplicate = this.value.find((config) => selected?.value === config.alias);

		if (duplicate) {
			this._selectEl.error = true;
			return;
		} else {
			this._selectEl.error = false;
		}

		const config: ColumnConfig = {
			alias: selected.value,
			header: selected.name,
			isSystem: selected?.group === 'System Fields',
		};
		this.value = [...this.value, config];

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onRemove(unique: string) {
		const newValue: Array<ColumnConfig> = [];
		this.value.forEach((config) => {
			if (config.alias !== unique) newValue.push(config);
		});
		this.value = newValue;
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onHeaderChange(e: UUIInputEvent, configuration: ColumnConfig) {
		this.value = this.value.map(
			(config): ColumnConfig =>
				config.alias === configuration.alias ? { ...config, header: e.target.value as string } : config,
		);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	#onTemplateChange(e: UUIInputEvent, configuration: ColumnConfig) {
		this.value = this.value.map(
			(config): ColumnConfig =>
				config.alias === configuration.alias ? { ...config, nameTemplate: e.target.value as string } : config,
		);
		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<div>
				<uui-select .options=${this._options} label="Select"></uui-select>
				<uui-button label=${this.localize.term('general_add')} look="secondary" @click=${this.#onAdd}></uui-button>
			</div>
			${this.#renderTable()}`;
	}

	#renderTable() {
		if (this.value && !this.value.length) return;
		return html`<uui-table>
			<uui-table-row>
				<uui-table-head-cell style="width:0"></uui-table-head-cell>
				<uui-table-head-cell>Alias</uui-table-head-cell>
				<uui-table-head-cell>Header</uui-table-head-cell>
				<uui-table-head-cell>Template</uui-table-head-cell>
				<uui-table-head-cell style="width:0"></uui-table-head-cell>
			</uui-table-row>
			${repeat(
				this.value,
				(configuration) => configuration.alias,
				(configuration) =>
					html`<uui-table-row>
						<uui-table-cell><uui-icon name="icon-navigation"></uui-icon></uui-table-cell>
						${configuration.isSystem
							? this.#renderSystemFieldRow(configuration)
							: this.#renderCustomFieldRow(configuration)}
						<uui-table-cell>
							<uui-button label="delete" look="secondary" @click=${() => this.#onRemove(configuration.alias)}
								>Remove</uui-button
							>
						</uui-table-cell>
					</uui-table-row> `,
			)}
		</uui-table>`;
	}

	#renderSystemFieldRow(configuration: ColumnConfig) {
		return html`
			<uui-table-cell><strong>${configuration.alias}</strong><small>(system field)</small></uui-table-cell>
			<uui-table-cell>${configuration.header}</uui-table-cell>
			<uui-table-cell></uui-table-cell>
		`;
	}

	#renderCustomFieldRow(configuration: ColumnConfig) {
		return html`
			<uui-table-cell><strong>${configuration.alias}</strong></uui-table-cell>
			<uui-table-cell>
				<uui-input
					label="header"
					.value=${configuration.header ?? ''}
					required
					@change=${(e: UUIInputEvent) => this.#onHeaderChange(e, configuration)}></uui-input>
			</uui-table-cell>
			<uui-table-cell>
				<uui-input
					label="template"
					.value=${configuration.nameTemplate ?? ''}
					@change=${(e: UUIInputEvent) => this.#onTemplateChange(e, configuration)}></uui-input>
			</uui-table-cell>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			strong {
				display: block;
			}
			uui-input {
				width: 100%;
			}
		`,
	];
}

export default UmbPropertyEditorUICollectionViewColumnConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-column-configuration': UmbPropertyEditorUICollectionViewColumnConfigurationElement;
	}
}
