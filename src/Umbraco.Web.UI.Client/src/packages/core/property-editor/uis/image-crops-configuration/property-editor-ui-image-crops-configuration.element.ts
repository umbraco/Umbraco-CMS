import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTableColumn, UmbTableConfig, UmbTableItem } from '@umbraco-cms/backoffice/components';
import './column-layouts/crops-table-input-column.element.js';

/**
 * @element umb-property-editor-ui-image-crops-configuration
 */
@customElement('umb-property-editor-ui-image-crops-configuration')
export class UmbPropertyEditorUIImageCropsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	@state()
	private _tableConfig: UmbTableConfig = {
		allowSelection: false,
	};

	@state()
	private _tableColumns: Array<UmbTableColumn> = [
		{
			name: 'Alias',
			alias: 'cropAlias',
			elementName: 'umb-crops-table-input-column',
		},
		{
			name: 'Width',
			alias: 'cropWidth',
			elementName: 'umb-crops-table-input-column',
		},
		{
			name: 'Height',
			alias: 'cropHeight',
			elementName: 'umb-crops-table-input-column',
		},
		{
			name: 'Actions',
			alias: 'cropActions',
		},
	];

	@state()
	private _tableItems: Array<UmbTableItem> = [
		{
			id: '1',
			icon: 'icon-image',
			entityType: 'Image',
			data: [
				{
					columnAlias: 'cropAlias',
					value: {
						value: 'test',
					},
				},
				{
					columnAlias: 'cropWidth',
					value: {
						value: 'test',
						append: 'px',
					},
				},
				{
					columnAlias: 'cropHeight',
					value: {
						value: 'test',
						append: 'px',
					},
				},
				{
					columnAlias: 'cropActions',
					value: 'test',
				},
			],
		},
	];

	render() {
		return html`
			<umb-table .config=${this._tableConfig} .columns=${this._tableColumns} .items=${this._tableItems}></umb-table>
		`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIImageCropsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-image-crops-configuration': UmbPropertyEditorUIImageCropsConfigurationElement;
	}
}
