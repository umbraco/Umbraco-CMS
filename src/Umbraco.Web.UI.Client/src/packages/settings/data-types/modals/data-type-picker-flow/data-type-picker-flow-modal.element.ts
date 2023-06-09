import { UmbDataTypeRepository } from '../../repository/data-type.repository.js';
import { css, html, repeat, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import {
	UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL,
	UMB_WORKSPACE_MODAL,
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalResult,
	UmbModalContext,
	UmbModalRouteBuilder,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';
import { ManifestPropertyEditorUi, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

interface GroupedItems<T> {
	[key: string]: Array<T>;
}
@customElement('umb-data-type-picker-flow-modal')
export class UmbDataTypePickerFlowModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbDataTypePickerFlowModalData, UmbDataTypePickerFlowModalResult>;

	@property({ type: Object })
	public get data(): UmbDataTypePickerFlowModalData | undefined {
		return this._data;
	}
	public set data(value: UmbDataTypePickerFlowModalData | undefined) {
		this._data = value;
		this._selection = this.data?.selection ?? [];
		this._submitLabel = this.data?.submitLabel ?? this._submitLabel;
	}
	private _data?: UmbDataTypePickerFlowModalData | undefined;

	@state()
	private _groupedDataTypes?: GroupedItems<EntityTreeItemResponseModel>;

	@state()
	private _groupedPropertyEditorUIs: GroupedItems<ManifestPropertyEditorUi> = {};

	@state()
	private _selection: Array<string> = [];

	@state()
	private _submitLabel = 'Select';

	@state()
	private _dataTypePickerModalRouteBuilder?: UmbModalRouteBuilder;

	private _createDataTypeModal: UmbModalRouteRegistrationController;

	#repository;
	#dataTypes: Array<EntityTreeItemResponseModel> = [];
	#propertyEditorUIs: Array<ManifestPropertyEditorUi> = [];
	#currentFilterQuery = '';

	constructor() {
		super();
		this.#repository = new UmbDataTypeRepository(this);

		new UmbModalRouteRegistrationController(this, UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((routingInfo) => {
				return {
					propertyEditorUiAlias: routingInfo.uiAlias,
				};
			})
			.onSubmit((submitData) => {
				if (submitData.dataTypeId) {
					this._select(submitData.dataTypeId);
					this._submit();
				} else if (submitData.createNewWithPropertyEditorUiAlias) {
					this._createDataType(submitData.createNewWithPropertyEditorUiAlias);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._dataTypePickerModalRouteBuilder = routeBuilder;
				this.requestUpdate('_dataTypePickerModalRouteBuilder');
			});

		this._createDataTypeModal = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((params) => {
				return { entityType: 'data-type', preset: { propertyEditorUiAlias: params.uiAlias } };
			})
			.onSubmit((submitData) => {
				this._select(submitData.id);
				this._submit();
			});

		this.#init();
	}

	private _createDataType(propertyEditorUiAlias: string) {
		// TODO: Could be nice with a more pretty way to prepend to the URL:
		// Open create modal:
		this._createDataTypeModal.open({ uiAlias: propertyEditorUiAlias }, 'create/null');
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

		this.observe(umbExtensionsRegistry.extensionsOfType('propertyEditorUi'), (propertyEditorUIs) => {
			this.#propertyEditorUIs = propertyEditorUIs;
			this._performFiltering();
		});
	}

	private _handleDataTypeClick(dataType: EntityTreeItemResponseModel) {
		if (dataType.id) {
			this._select(dataType.id);
			this._submit();
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
		this.modalContext?.reject();
	}

	private _submit() {
		this.modalContext?.submit({ selection: this._selection });
	}

	render() {
		return html`
			<umb-body-layout headline="Select editor">
				<uui-box> ${this._renderFilter()} ${this._renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label="Close" @click=${this._close}></uui-button>
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

	private _renderGroupUIs(uis: Array<ManifestPropertyEditorUi>) {
		return html` <ul id="item-grid">
			${this._dataTypePickerModalRouteBuilder
				? repeat(
						uis,
						(propertyEditorUI) => propertyEditorUI.alias,
						(propertyEditorUI) => html` <li class="item">
							<a type="button" href=${this._dataTypePickerModalRouteBuilder!({ uiAlias: propertyEditorUI.alias })}>
								<uui-icon name="${propertyEditorUI.meta.icon}" class="icon"></uui-icon>
								${propertyEditorUI.meta.label || propertyEditorUI.name}
							</a>
						</li>`
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

			#item-grid .item {
				list-style: none;
				height: 100%;
				width: 100%;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);
				box-sizing: border-box;
				color: var(--uui-color-interactive);
			}

			#item-grid .item:hover {
				background: var(--uui-color-surface-emphasis);
				color: var(--uui-color-interactive-emphasis);
				cursor: pointer;
			}

			#item-grid .item a {
				border: none;
				padding: 0;
				margin: 0;
				color: inherit;
				text-decoration: none;
				text-align: center;
				box-sizing: border-box;

				padding: var(--uui-size-space-2);

				display: grid;
				grid-template-rows: 40px 1fr;
				height: 100%;
				width: 100%;
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin: auto;
				/* width: fit-content;
				height: fit-content; */
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
