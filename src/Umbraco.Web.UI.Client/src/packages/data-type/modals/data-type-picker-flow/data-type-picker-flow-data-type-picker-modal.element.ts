import { UmbDataTypeCollectionRepository } from '../../collection/index.js';
import type { UmbDataTypeItemModel } from '../../repository/item/types.js';
import type {
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalValue,
} from './data-type-picker-flow-data-type-picker-modal.token.js';
import { css, customElement, html, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-data-type-picker-flow-data-type-picker-modal')
export class UmbDataTypePickerFlowDataTypePickerModalElement extends UmbModalBaseElement<
	UmbDataTypePickerFlowDataTypePickerModalData,
	UmbDataTypePickerFlowDataTypePickerModalValue
> {
	@state()
	private _dataTypes?: Array<UmbDataTypeItemModel>;

	private _propertyEditorUiAlias!: string;

	override connectedCallback(): void {
		super.connectedCallback();

		if (!this.data) return;

		this._propertyEditorUiAlias = this.data.propertyEditorUiAlias;

		this.#observeDataTypesOf(this._propertyEditorUiAlias);
	}

	async #observeDataTypesOf(propertyEditorUiAlias: string) {
		if (!this.data) return;

		const dataTypeCollectionRepository = new UmbDataTypeCollectionRepository(this);

		const collection = await dataTypeCollectionRepository.requestCollection({
			skip: 0,
			take: 1000,
			editorUiAlias: propertyEditorUiAlias,
		});

		this.observe(collection.asObservable(), (dataTypes) => {
			this._dataTypes = dataTypes.sort((a, b) => a.name.localeCompare(b.name));
		});
	}

	#handleClick(dataType: UmbDataTypeItemModel) {
		if (dataType.unique) {
			this.value = { dataTypeId: dataType.unique };
			this.modalContext?.submit();
		}
	}

	#handleCreate() {
		this.value = { createNewWithPropertyEditorUiAlias: this._propertyEditorUiAlias };
		this.modalContext?.submit();
	}

	#close() {
		this.modalContext?.reject();
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectEditorConfiguration')}>
				<uui-box>${this.#renderDataTypes()} ${this.#renderCreate()}</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this.#close}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderDataTypes() {
		if (!this._dataTypes?.length) return;
		return html`
			<ul id="item-grid">
				${repeat(
					this._dataTypes,
					(dataType) => dataType.unique,
					(dataType) => html`
						<li class="item">
							<uui-button label=${dataType.name} @click=${() => this.#handleClick(dataType)}>
								<div class="item-content">
									<umb-icon name=${dataType.icon ?? 'icon-circle-dotted'} class="icon"></umb-icon>
									${dataType.name}
								</div>
							</uui-button>
						</li>
					`,
				)}
			</ul>
		`;
	}

	#renderCreate() {
		return html`
			<uui-button id="create-button" look="placeholder" @click=${this.#handleCreate}>
				<div class="content">
					<uui-icon name="icon-add" class="icon"></uui-icon>
					<umb-localize key="contentTypeEditor_availableEditors">Create new</umb-localize>
				</div>
			</uui-button>
		`;
	}

	static override styles = [
		UmbTextStyles,
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
				word-break: break-word;
			}
			.icon {
				font-size: 2em;
				margin: auto;
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
