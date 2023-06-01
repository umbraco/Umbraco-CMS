import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalResult,
	UmbModalHandler,
	UmbDataTypePickerFlowDataTypePickerModalData,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-data-type-picker-flow-data-type-picker-modal')
export class UmbDataTypePickerFlowDataTypePickerModalElement extends UmbLitElement {
	@property({ type: Object })
	data?: UmbDataTypePickerFlowDataTypePickerModalData;

	@state()
	private _dataTypes?: Array<FolderTreeItemResponseModel>;

	@state()
	private _selection: Array<string> = [];

	connectedCallback(): void {
		super.connectedCallback();

		if (!this.data) return;

		this._selection = this.data.selection ?? [];
		this._observeDataTypesOf(this.data.propertyEditorUiAlias);
	}

	private async _observeDataTypesOf(propertyEditorUiAlias: string) {
		if (!this.data) return;

		const dataTypeRepository = new UmbDataTypeRepository(this);

		// TODO: make an end-point just retrieving the data types using a given property editor ui alias.
		await dataTypeRepository.requestRootTreeItems();

		// TODO: Use the asObservable from above onces end-point has been made.
		const source = await dataTypeRepository.treeItemsByPropertyEditorUiAlias(propertyEditorUiAlias);
		this.observe(source, (dataTypes) => {
			this._dataTypes = dataTypes;
		});
	}

	private _handleClick(dataType: FolderTreeItemResponseModel) {
		if (dataType.id) {
			this._select(dataType.id);
		}
	}

	private _select(alias: string) {
		this._selection = [alias];
	}

	private _close() {
		this.modalHandler?.reject();
	}

	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbPropertyEditorUIPickerModalData, UmbPropertyEditorUIPickerModalResult>;

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	render() {
		return html`
			<umb-body-layout headline="Select a configuration">
				<uui-box> ${this._renderDataTypes()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	private _renderDataTypes() {
		return html` <ul id="item-grid">
			${this._dataTypes
				? repeat(
						this._dataTypes,
						(dataType) => dataType.id,
						(dataType) =>
							dataType.id
								? html` <li class="item" ?selected=${this._selection.includes(dataType.id)}>
										<button type="button" @click="${() => this._handleClick(dataType)}">
											<uui-icon name="${dataType.icon}" class="icon"></uui-icon>
											${dataType.name}
										</button>
								  </li>`
								: ''
				  )
				: ''}
		</ul>`;
	}

	static styles = [
		UUITextStyles,
		css`
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				padding-left: var(--uui-size-space-2);
			}

			#item-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(70px, 1fr));
				margin: 0;
				padding: 0;
				grid-gap: var(--uui-size-space-4);
			}

			#item-grid .item {
				display: flex;
				align-items: flex-start;
				justify-content: center;
				list-style: none;
				height: 100%;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				cursor: pointer;
			}

			#item-grid .item[selected] button {
				background: var(--uui-color-selected);
				color: var(--uui-color-selected-contrast);
			}

			#item-grid .item button {
				background: none;
				border: none;
				cursor: pointer;
				padding: var(--uui-size-space-3);
				display: flex;
				align-items: center;
				flex-direction: column;
				justify-content: center;
				font-size: 0.8rem;
				height: 100%;
				width: 100%;
				color: var(--uui-color-interactive);
				border-radius: var(--uui-border-radius);
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin-bottom: var(--uui-size-space-2);
			}
		`,
	];
}

export default UmbDataTypePickerFlowDataTypePickerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-picker-flow-data-type-picker-modal': UmbDataTypePickerFlowDataTypePickerModalElement;
	}
}
