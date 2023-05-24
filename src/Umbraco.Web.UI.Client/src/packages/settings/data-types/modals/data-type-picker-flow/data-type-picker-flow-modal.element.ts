import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { repeat } from '@umbraco-cms/backoffice/external/lit';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';
import type { UUIInputEvent } from '@umbraco-ui/uui';
import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import {
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalResult,
	UmbModalHandler,
} from '@umbraco-cms/backoffice/modal';
import { ManifestPropertyEditorUI, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

interface GroupedItems<T> {
	[key: string]: Array<T>;
}
@customElement('umb-data-type-picker-flow-modal')
export class UmbDataTypePickerFlowModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalHandler?: UmbModalHandler<UmbPropertyEditorUIPickerModalData, UmbPropertyEditorUIPickerModalResult>;

	@property({ type: Object })
	public get data(): UmbPropertyEditorUIPickerModalData | undefined {
		return this._data;
	}
	public set data(value: UmbPropertyEditorUIPickerModalData | undefined) {
		this._data = value;
		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;
	}
	private _data?: UmbPropertyEditorUIPickerModalData | undefined;

	@state()
	private _groupedDataTypes?: GroupedItems<EntityTreeItemResponseModel>;

	@state()
	private _groupedPropertyEditorUIs: GroupedItems<ManifestPropertyEditorUI> = {};

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	#repository;
	#dataTypes: Array<EntityTreeItemResponseModel> = [];
	#propertyEditorUIs: Array<ManifestPropertyEditorUI> = [];
	#currentFilterQuery = '';

	constructor() {
		super();
		this.#repository = new UmbDataTypeRepository(this);

		this.#init();
	}

	async #init() {
		// TODO: Get ALL items, or traverse the structure aka. multiple recursive calls.
		this.observe(
			(await this.#repository.requestRootTreeItems()).asObservable(),
			(items) => {
				this.#dataTypes = items;
				this._performFiltering();
			},
			'_repositoryItemsObserver'
		);

		this.observe(umbExtensionsRegistry.extensionsOfType('propertyEditorUI'), (propertyEditorUIs) => {
			this.#propertyEditorUIs = propertyEditorUIs;
			this._performFiltering();
		});
	}

	private _handleUIClick(propertyEditorUI: ManifestPropertyEditorUI) {
		alert('To BE DONE.');
	}

	private _handleDataTypeClick(dataType: EntityTreeItemResponseModel) {
		if (dataType.id) {
			this._select(dataType.id);
		}
	}

	private _select(id: string) {
		this._selection = [id];
	}

	private _handleFilterInput(event: UUIInputEvent) {
		const query = (event.target.value as string) || '';
		this.#currentFilterQuery = query.toLowerCase();
		this._performFiltering();
	}
	private _performFiltering() {
		if (this.#currentFilterQuery) {
			this._groupedDataTypes = groupBy(
				this.#dataTypes.filter((dataType) => {
					return dataType.name?.toLowerCase().includes(this.#currentFilterQuery);
				}),
				'meta.group'
			);
		} else {
			this._groupedDataTypes = undefined;
		}

		const filteredUIs = !this.#currentFilterQuery
			? this.#propertyEditorUIs
			: this.#propertyEditorUIs.filter((propertyEditorUI) => {
					return (
						propertyEditorUI.name.toLowerCase().includes(this.#currentFilterQuery) ||
						propertyEditorUI.alias.toLowerCase().includes(this.#currentFilterQuery)
					);
			  });

		this._groupedPropertyEditorUIs = groupBy(filteredUIs, 'meta.group');
	}

	private _close() {
		this.modalHandler?.reject();
	}

	private _submit() {
		this.modalHandler?.submit({ selection: this._selection });
	}

	render() {
		return html`
			<umb-body-layout headline="Select editor">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
					<uui-button label="${this._submitLabel}" look="primary" color="positive" @click=${this._submit}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	private _renderGrid() {
		return this.#currentFilterQuery
			? html`
					<h5>Available configurations</h5>
					${this._renderDataTypes()}
					<h5>Create a new configuration</h5>
					${this._renderUIs()}
			  `
			: html`${this._renderUIs()}`;
	}

	private _renderFilter() {
		return html` <uui-input
			id="filter"
			@input="${this._handleFilterInput}"
			placeholder="Type to filter..."
			label="Type to filter icons">
			<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
		</uui-input>`;
	}

	private _renderDataTypes() {
		return this._groupedDataTypes
			? html` ${Object.entries(this._groupedDataTypes).map(
					([key, value]) =>
						html` <h4>${key}</h4>
							${this._renderGroupDataTypes(value)}`
			  )}`
			: '';
	}

	private _renderGroupDataTypes(dataTypes: Array<EntityTreeItemResponseModel>) {
		return html` <ul id="item-grid">
			${repeat(
				dataTypes,
				(dataType) => dataType.id,
				(dataType) => html`<li class="item" ?selected=${this._selection.includes(dataType.id!)}>
					<button type="button" @click="${() => this._handleDataTypeClick(dataType)}">
						<uui-icon name="${dataType.icon}" class="icon"></uui-icon>
						${dataType.name}
					</button>
				</li>`
			)}
		</ul>`;
	}

	private _renderUIs() {
		return html` ${Object.entries(this._groupedPropertyEditorUIs).map(
			([key, value]) =>
				html` <h4>${key}</h4>
					${this._renderGroupUIs(value)}`
		)}`;
	}

	private _renderGroupUIs(uis: Array<ManifestPropertyEditorUI>) {
		return html` <ul id="item-grid">
			${repeat(
				uis,
				(propertyEditorUI) => propertyEditorUI.alias,
				(propertyEditorUI) => html` <li class="item">
					<button type="button" @click="${() => this._handleUIClick(propertyEditorUI)}">
						<uui-icon name="${propertyEditorUI.meta.icon}" class="icon"></uui-icon>
						${propertyEditorUI.meta.label || propertyEditorUI.name}
					</button>
				</li>`
			)}
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

export default UmbDataTypePickerFlowModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-data-type-picker-flow-modal': UmbDataTypePickerFlowModalElement;
	}
}
