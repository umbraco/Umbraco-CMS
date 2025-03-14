import { UMB_DATATYPE_WORKSPACE_MODAL } from '../../workspace/data-type-workspace.modal-token.js';
import { UMB_DATA_TYPE_ENTITY_TYPE, UMB_DATA_TYPE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbDataTypeCollectionRepository } from '../../collection/index.js';
import type { UmbDataTypeItemModel } from '../../repository/index.js';
import { UMB_CREATE_DATA_TYPE_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL } from './data-type-picker-flow-data-type-picker-modal.token.js';
import type {
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalValue,
} from './data-type-picker-flow-modal.token.js';
import { css, customElement, html, nothing, repeat, state, when } from '@umbraco-cms/backoffice/external/lit';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbPaginationManager, debounce, fromCamelCase } from '@umbraco-cms/backoffice/utils';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import { UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/property-type';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';
import type { UmbModalRouteBuilder } from '@umbraco-cms/backoffice/router';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-data-type-picker-flow-modal')
export class UmbDataTypePickerFlowModalElement extends UmbModalBaseElement<
	UmbDataTypePickerFlowModalData,
	UmbDataTypePickerFlowModalValue
> {
	#initPromise!: Promise<unknown>;

	public override set data(value: UmbDataTypePickerFlowModalData) {
		super.data = value;
	}

	@state()
	private _groupedDataTypes?: Array<{ key: string; items: Array<UmbDataTypeItemModel> }> = [];

	@state()
	private _groupedPropertyEditorUIs: Array<{ key: string; items: Array<ManifestPropertyEditorUi> }> = [];

	@state()
	private _currentPage = 1;

	@state()
	private _dataTypePickerModalRouteBuilder?: UmbModalRouteBuilder;

	pagination = new UmbPaginationManager();

	#collectionRepository;

	#createDataTypeModal!: UmbModalRouteRegistrationController<
		typeof UMB_DATATYPE_WORKSPACE_MODAL.DATA,
		typeof UMB_DATATYPE_WORKSPACE_MODAL.VALUE
	>;

	#currentFilterQuery = '';

	#dataTypes: Array<UmbDataTypeItemModel> = [];

	#groupLookup: Record<string, string> = {};

	#propertyEditorUIs: Array<ManifestPropertyEditorUi> = [];

	constructor() {
		super();

		this.#collectionRepository = new UmbDataTypeCollectionRepository(this);
		this.#init();
	}

	#createDataType(propertyEditorUiAlias: string) {
		const createPath = UMB_CREATE_DATA_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({
			parentEntityType: UMB_DATA_TYPE_ROOT_ENTITY_TYPE,
			parentUnique: null,
		});

		// TODO: Could be nice with a more pretty way to prepend to the URL:
		// Open create modal:
		this.#createDataTypeModal.open({ uiAlias: propertyEditorUiAlias }, createPath);
	}

	async #init() {
		this.pagination.setCurrentPageNumber(1);
		this.pagination.setPageSize(100);

		this.#initPromise = Promise.all([
			this.observe(umbExtensionsRegistry.byType('propertyEditorUi'), (propertyEditorUIs) => {
				// Only include Property Editor UIs which has Property Editor Schema Alias
				this.#propertyEditorUIs = propertyEditorUIs
					.filter((propertyEditorUi) => !!propertyEditorUi.meta.propertyEditorSchemaAlias)
					.sort((a, b) => a.meta.label.localeCompare(b.meta.label));

				this.#groupLookup = Object.fromEntries(propertyEditorUIs.map((ui) => [ui.alias, ui.meta.group]));

				this.#performFiltering();
			}).asPromise(),
		]);

		new UmbModalRouteRegistrationController(this, UMB_DATA_TYPE_PICKER_FLOW_DATA_TYPE_PICKER_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup((routingInfo) => {
				return {
					data: {
						propertyEditorUiAlias: routingInfo.uiAlias,
					},
					value: undefined,
				};
			})
			.onSubmit((submitData) => {
				if (submitData?.dataTypeId) {
					this.#select(submitData.dataTypeId);
					this._submitModal();
				} else if (submitData?.createNewWithPropertyEditorUiAlias) {
					this.#createDataType(submitData.createNewWithPropertyEditorUiAlias);
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				this._dataTypePickerModalRouteBuilder = routeBuilder;
				this.requestUpdate('_dataTypePickerModalRouteBuilder');
			});

		this.#createDataTypeModal = new UmbModalRouteRegistrationController(this, UMB_DATATYPE_WORKSPACE_MODAL)
			.addAdditionalPath(':uiAlias')
			.onSetup(async (params) => {
				const contentContextConsumer = this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, () => {
					this.removeUmbController(contentContextConsumer);
				}).passContextAliasMatches();
				const propContextConsumer = this.consumeContext(UMB_PROPERTY_TYPE_WORKSPACE_CONTEXT, () => {
					this.removeUmbController(propContextConsumer);
				}).passContextAliasMatches();
				const [contentContext, propContext] = await Promise.all([
					contentContextConsumer.asPromise({ preventTimeout: true }),
					propContextConsumer.asPromise({ preventTimeout: true }),
					this.#initPromise,
				]);
				if (!contentContext || !propContext) {
					throw new Error('Could not get content or property context');
				}
				const propertyEditorName = this.#propertyEditorUIs.find((ui) => ui.alias === params.uiAlias)?.name;
				const dataTypeName = `${contentContext?.getName() ?? ''} - ${propContext.getName() ?? ''} - ${propertyEditorName}`;

				return {
					data: {
						entityType: UMB_DATA_TYPE_ENTITY_TYPE,
						preset: { editorUiAlias: params.uiAlias, name: dataTypeName },
					},
				};
			})
			.onSubmit((value) => {
				this.#select(value?.unique);
				this._submitModal();
			});
	}

	async #getDataTypes() {
		this.pagination.setCurrentPageNumber(this._currentPage);

		const { data } = await this.#collectionRepository.requestCollection({
			skip: this.pagination.getSkip(),
			take: this.pagination.getPageSize(),
			name: this.#currentFilterQuery,
		});

		this.pagination.setTotalItems(data?.total ?? 0);

		if (this.pagination.getCurrentPageNumber() > 1) {
			this.#dataTypes = [...this.#dataTypes, ...(data?.items ?? [])];
		} else {
			this.#dataTypes = data?.items ?? [];
		}
	}

	#handleDataTypeClick(dataType: UmbDataTypeItemModel) {
		if (dataType.unique) {
			this.#select(dataType.unique);
			this._submitModal();
		}
	}

	#select(unique: string | undefined) {
		this.value = { selection: unique ? [unique] : [] };
	}

	async #onLoadMore() {
		this._currentPage = this._currentPage + 1;
		this.#handleFiltering();
	}

	#onFilterInput(event: UUIInputEvent) {
		this.#currentFilterQuery = (event.target.value as string).toLocaleLowerCase();
		this.#debouncedFilterInput();
	}

	#debouncedFilterInput = debounce(() => {
		this._currentPage = 1;
		this.#handleFiltering();
	}, 250);

	async #handleFiltering() {
		await this.#getDataTypes();
		this.#performFiltering();
	}

	#performFiltering() {
		if (this.#currentFilterQuery) {
			const filteredDataTypes = this.#dataTypes
				.filter((dataType) => dataType.name?.toLowerCase().includes(this.#currentFilterQuery))
				.sort((a, b) => a.name.localeCompare(b.name));

			// eslint-disable-next-line @typescript-eslint/ban-ts-comment
			// @ts-expect-error
			const grouped = Object.groupBy(filteredDataTypes, (dataType: UmbDataTypeItemModel) =>
				fromCamelCase(this.#groupLookup[dataType.propertyEditorUiAlias] ?? 'Uncategorized'),
			);

			this._groupedDataTypes = Object.keys(grouped)
				.sort((a, b) => a.localeCompare(b))
				.map((key) => ({ key, items: grouped[key] }));
		} else {
			this._groupedDataTypes = [];
		}

		const filteredUIs = !this.#currentFilterQuery
			? this.#propertyEditorUIs
			: this.#propertyEditorUIs.filter(
					(propertyEditorUI) =>
						propertyEditorUI.name.toLowerCase().includes(this.#currentFilterQuery) ||
						propertyEditorUI.alias.toLowerCase().includes(this.#currentFilterQuery),
				);

		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		const grouped = Object.groupBy(filteredUIs, (propertyEditorUi: ManifestPropertyEditorUi) =>
			fromCamelCase(propertyEditorUi.meta.group ?? 'Uncategorized'),
		);

		this._groupedPropertyEditorUIs = Object.keys(grouped)
			.sort((a, b) => a.localeCompare(b))
			.map((key) => ({ key, items: grouped[key] }));
	}

	override render() {
		return html`
			<umb-body-layout headline=${this.localize.term('defaultdialogs_selectEditor')} class="uui-text">
				<uui-box> ${this.#renderFilter()} ${this.#renderGrid()} </uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	#renderGrid() {
		return this.#currentFilterQuery ? this.#renderFilteredList() : this.#renderUIs();
	}

	#renderFilter() {
		return html` <uui-input
			type="search"
			id="filter"
			@input=${this.#onFilterInput}
			placeholder=${this.localize.term('placeholders_filter')}
			label=${this.localize.term('placeholders_filter')}
			${umbFocus()}>
			<uui-icon name="search" slot="prepend" id="filter-icon"></uui-icon>
		</uui-input>`;
	}

	#renderFilteredList() {
		if (!this._groupedDataTypes) return nothing;
		if (!this._groupedPropertyEditorUIs) return nothing;

		if (this._groupedDataTypes.length === 0 && this._groupedPropertyEditorUIs.length === 0) {
			return html`<p>Nothing matches your search, try another search term.</p>`;
		}

		return html`
			${when(
				this._groupedDataTypes.length > 0,
				() => html`
					<h5 class="choice-type-headline">
						<umb-localize key="contentTypeEditor_searchResultSettings">Available configurations</umb-localize>
					</h5>
					${this.#renderDataTypes()} ${this.#renderLoadMore()}
				`,
			)}
			${when(
				this._groupedPropertyEditorUIs.length > 0,
				() => html`
					<h5 class="choice-type-headline">
						<umb-localize key="contentTypeEditor_searchResultEditors">Create a new configuration</umb-localize>
					</h5>
					${this.#renderUIs(true)}
				`,
			)}
		`;
	}

	#renderLoadMore() {
		if (this._currentPage >= this.pagination.getTotalPages()) return;
		return html`<uui-button @click=${this.#onLoadMore} look="secondary" label="Load more"></uui-button>`;
	}

	#renderDataTypes() {
		if (!this._groupedDataTypes) return nothing;

		// TODO: Fix so we can have Data Types grouped. (or choose not to group them)
		return this._groupedDataTypes.map(
			(group) => html`
				<h5 class="category-name">${group.key}</h5>
				${this.#renderGroupDataTypes(group.items)}
			`,
		);
	}

	#renderUIs(createAsNewOnPick?: boolean) {
		if (!this._groupedPropertyEditorUIs) return nothing;

		return this._groupedPropertyEditorUIs.map(
			(group) => html`
				<h5 class="category-name">${group.key}</h5>
				${this.#renderGroupUIs(group.items, createAsNewOnPick)}
			`,
		);
	}

	#renderGroupUIs(uis: Array<ManifestPropertyEditorUi>, createAsNewOnPick?: boolean) {
		return html`
			<ul id="item-grid">
				${this._dataTypePickerModalRouteBuilder
					? repeat(
							uis,
							(propertyEditorUI) => propertyEditorUI.alias,
							(propertyEditorUI) => {
								return html`<li class="item">${this.#renderDataTypeButton(propertyEditorUI, createAsNewOnPick)}</li>`;
							},
						)
					: ''}
			</ul>
		`;
	}

	#renderDataTypeButton(propertyEditorUI: ManifestPropertyEditorUi, createAsNewOnPick?: boolean) {
		if (createAsNewOnPick) {
			return html`
				<uui-button
					label=${propertyEditorUI.meta.label || propertyEditorUI.name}
					@click=${() => this.#createDataType(propertyEditorUI.alias)}>
					${this.#renderItemContent(propertyEditorUI)}
				</uui-button>
			`;
		} else {
			return html`
				<uui-button
					label=${propertyEditorUI.meta.label || propertyEditorUI.name}
					href=${this._dataTypePickerModalRouteBuilder!({ uiAlias: propertyEditorUI.alias })}>
					${this.#renderItemContent(propertyEditorUI)}
				</uui-button>
			`;
		}
	}

	#renderItemContent(propertyEditorUI: ManifestPropertyEditorUi) {
		return html`
			<div class="item-content">
				<umb-icon name=${propertyEditorUI.meta.icon} class="icon"></umb-icon>
				${propertyEditorUI.meta.label || propertyEditorUI.name}
			</div>
		`;
	}

	#renderGroupDataTypes(dataTypes: Array<UmbDataTypeItemModel>) {
		return html`
			<ul id="item-grid">
				${repeat(
					dataTypes,
					(dataType) => dataType.unique,
					(dataType) => html`
						<li class="item" ?selected=${this.value.selection.includes(dataType.unique)}>
							<uui-button .label=${dataType.name} type="button" @click=${() => this.#handleDataTypeClick(dataType)}>
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

	static override styles = [
		UmbTextStyles,
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

			#item-grid:not(:last-child) {
				padding-bottom: var(--uui-size-space-5);
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

			#item-grid .item uui-button {
				--uui-button-padding-left-factor: 0;
				--uui-button-padding-right-factor: 0;
				--uui-button-padding-top-factor: 0;
				--uui-button-padding-bottom-factor: 0;
				width: 100%;
				height: 100%;
			}

			#item-grid .item .item-content {
				text-align: center;
				box-sizing: border-box;

				padding: var(--uui-size-space-2);

				display: grid;
				grid-template-rows: 40px 1fr;
				height: 100%;
				width: 100%;
				word-break: break-word;
			}

			#item-grid .item .icon {
				font-size: 2em;
				margin: auto;
			}

			.category-name {
				text-transform: capitalize;
			}

			.choice-type-headline {
				border-bottom: 1px solid var(--uui-color-divider);
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
