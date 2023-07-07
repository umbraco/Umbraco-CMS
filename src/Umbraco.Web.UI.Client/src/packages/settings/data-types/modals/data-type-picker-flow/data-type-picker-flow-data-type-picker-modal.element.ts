import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import { css, html, customElement, property, state, repeat, when } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import {
	UmbModalContext,
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalResult,
} from '@umbraco-cms/backoffice/modal';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { FolderTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-data-type-picker-flow-data-type-picker-modal')
export class UmbDataTypePickerFlowDataTypePickerModalElement extends UmbLitElement {
	@property({ type: Object })
	data?: UmbDataTypePickerFlowDataTypePickerModalData;

	@state()
	private _dataTypes?: Array<FolderTreeItemResponseModel>;

	private _propertyEditorUiAlias!: string;

	connectedCallback(): void {
		super.connectedCallback();

		if (!this.data) return;

		this._propertyEditorUiAlias = this.data.propertyEditorUiAlias;

		this._observeDataTypesOf(this._propertyEditorUiAlias);
	}

	private async _observeDataTypesOf(propertyEditorUiAlias: string) {
		if (!this.data) return;

		const dataTypeRepository = new UmbDataTypeRepository(this);

		// TODO: This is a hack to get the data types of a property editor ui alias.
		// TODO: Make sure filtering works data-type that does not have a property editor ui, but should be using the default property editor UI for those.
		// TODO: make an end-point just retrieving the data types using a given property editor ui alias.
		const { data } = await dataTypeRepository.requestRootTreeItems();

		if (!data) return;

		await Promise.all(
			data.items.map((item) => {
				if (item.id) {
					return dataTypeRepository.requestById(item.id);
				}
				return Promise.resolve();
			})
		);

		// TODO: Use the asObservable from above onces end-point has been made.
		const source = await dataTypeRepository.byPropertyEditorUiAlias(propertyEditorUiAlias);
		this.observe(source, (dataTypes) => {
			this._dataTypes = dataTypes;
		});
	}

	private _handleClick(dataType: FolderTreeItemResponseModel) {
		if (dataType.id) {
			this.modalContext?.submit({ dataTypeId: dataType.id });
		}
	}

	private _handleCreate() {
		this.modalContext?.submit({ createNewWithPropertyEditorUiAlias: this._propertyEditorUiAlias });
	}

	private _close() {
		this.modalContext?.reject();
	}

	@property({ attribute: false })
	modalContext?: UmbModalContext<
		UmbDataTypePickerFlowDataTypePickerModalData,
		UmbDataTypePickerFlowDataTypePickerModalResult
	>;

	render() {
		return html`
			<umb-body-layout headline="Select a configuration">
				<uui-box> ${this._renderDataTypes()} ${this._renderCreate()}</uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	private _renderDataTypes() {
		const shouldRender = this._dataTypes && this._dataTypes.length > 0;
		console.log(this._dataTypes, 'yo', shouldRender);

		return when(
			shouldRender,
			() =>
				html`<ul id="item-grid">
					${this._dataTypes
						? repeat(
								this._dataTypes,
								(dataType) => dataType.id,
								(dataType) =>
									dataType.id
										? html` <li class="item">
												<uui-button label="dataType.name" type="button" @click="${() => this._handleClick(dataType)}">
													<div class="item-content">
														<uui-icon name="${'umb:bug'}" class="icon"></uui-icon>
														${dataType.name}
													</div>
												</uui-button>
										  </li>`
										: ''
						  )
						: ''}
				</ul>`
		);
	}
	private _renderCreate() {
		return html`
			<uui-button id="create-button" type="button" look="placeholder" @click="${() => this._handleCreate()}">
				<div class="content">
					<uui-icon name="umb:add" class="icon"></uui-icon>
					Create new
				</div>
			</uui-button>
		`;
	}

	static styles = [
		UUITextStyles,
		css`
			uui-box {
				min-height: 100%;
			}
			#filter {
				width: 100%;
				margin-bottom: var(--uui-size-space-4);
			}

			#filter-icon {
				height: 100%;
				padding-left: var(--uui-size-space-2);
				display: flex;
				color: var(--uui-color-border);
			}

			#item-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(100px, 1fr));
				margin: 0;
				padding: 0;
			}

			#item-grid:not(:last-child) {
				border-bottom: 1px solid var(--uui-color-divider);
				padding-bottom: var(--uui-size-space-5);
			}

			.item {
				list-style: none;
				height: 100%;
				width: 100%;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);
				box-sizing: border-box;
				color: var(--uui-color-interactive);
			}

			.item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				cursor: pointer;
			}

			.item uui-button {
				--uui-button-padding-left-factor: 0;
				--uui-button-padding-right-factor: 0;
				--uui-button-padding-top-factor: 0;
				--uui-button-padding-bottom-factor: 0;
				width: 100%;
				height: 100%;
			}

			.item .item-content {
				text-align: center;
				box-sizing: border-box;

				padding: var(--uui-size-space-2);

				display: grid;
				grid-template-rows: 40px 1fr;
				height: 100%;
				width: 100%;
			}
			.icon {
				font-size: 2em;
				margin: auto;
			}

			#category-name {
				text-align: center;
				display: block;
				text-transform: capitalize;
				font-size: 1.2rem;
			}
			#create-button {
				max-width: 100px;
				--uui-button-padding-left-factor: 0;
				--uui-button-padding-right-factor: 0;
				--uui-button-padding-top-factor: 0;
				--uui-button-padding-bottom-factor: 0;
				width: 100%;
				height: 100%;
			}
			#create-button .content {
				text-align: center;
				box-sizing: border-box;

				padding: var(--uui-size-space-2);

				display: grid;
				grid-template-rows: 40px 1fr;
				height: 100%;
				width: 100%;
			}
			#create-button:not(:first-child) {
				margin-top: var(--uui-size-layout-1);
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
