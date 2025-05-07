import type { UmbBlockTypeBaseModel, UmbBlockTypeWithGroupKey } from '../../types.js';
import type { UmbBlockTypeCardElement } from '../block-type-card/index.js';
import { umbConfirmModal } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import {
	UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT,
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	type UmbDocumentTypePickerModalData,
	type UmbDocumentTypePickerModalValue,
} from '@umbraco-cms/backoffice/document-type';
import { UmbSorterController, UmbSorterResolvePlacementAsGrid } from '@umbraco-cms/backoffice/sorter';

import '../block-type-card/index.js';

/** TODO: Look into sending a "change" event when there is a change, rather than create, delete, and change event. Make sure it doesn't break move for RTE/List/Grid. [LI] */
@customElement('umb-input-block-type')
export class UmbInputBlockTypeElement<
	BlockType extends UmbBlockTypeWithGroupKey = UmbBlockTypeWithGroupKey,
> extends UmbLitElement {
	#sorter = new UmbSorterController<BlockType, UmbBlockTypeCardElement>(this, {
		getUniqueOfElement: (element) => element.contentElementTypeKey,
		getUniqueOfModel: (modelEntry) => modelEntry.contentElementTypeKey!,
		itemSelector: 'umb-block-type-card',
		identifier: 'umb-block-type-sorter',
		containerSelector: '#blocks',
		resolvePlacement: UmbSorterResolvePlacementAsGrid,
		onContainerChange: ({ item, model }) => {
			this.dispatchEvent(new CustomEvent('container-change', { detail: { item, model } }));
		},
		onChange: ({ model }) => {
			this._value = model;
			this.dispatchEvent(new UmbChangeEvent());
		},
		/*onEnd: () => {
			// TODO: Investigate if onEnd is called when a container move has been performed, if not then I would say it should be. [NL]
			this.dispatchEvent(new UmbChangeEvent());
		},*/
	});
	#elementPickerModal: UmbModalRouteRegistrationController<
		UmbDocumentTypePickerModalData,
		UmbDocumentTypePickerModalValue
	>;

	@property({ type: Array, attribute: false })
	public set value(items) {
		this._value = items ?? [];
		// Make sure the block types are unique on contentTypeElementKey:
		this._value = this._value.filter(
			(value, index, self) => self.findIndex((x) => x.contentElementTypeKey === value.contentElementTypeKey) === index,
		);
		this.#sorter.setModel(this._value);
	}
	public get value() {
		return this._value;
	}

	@property({ type: String })
	public set propertyAlias(value: string | undefined) {
		this.#elementPickerModal.setUniquePathValue('propertyAlias', value);
	}
	public get propertyAlias(): string | undefined {
		return undefined;
	}

	@property({ type: String })
	workspacePath?: string;

	@state()
	private _pickerPath?: string;

	@state()
	private _value: Array<BlockType> = [];

	// TODO: Seems no need to have these initially, then can be retrieved inside the `create` method. [NL]
	#datasetContext?: UmbPropertyDatasetContext;
	#filter: Array<UmbBlockTypeBaseModel> = [];

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#datasetContext = instance;
			this.observe(
				await this.#datasetContext?.propertyValueByAlias('blocks'),
				(value) => {
					this.#filter = value as Array<UmbBlockTypeBaseModel>;
				},
				'observeBlocks',
			);
		});

		this.#elementPickerModal = new UmbModalRouteRegistrationController(this, UMB_DOCUMENT_TYPE_PICKER_MODAL)
			.addUniquePaths(['propertyAlias'])
			.onSetup(() => {
				return {
					data: {
						hideTreeRoot: true,
						multiple: false,
						createAction: {
							extendWithPathParams: {
								parentUnique: null,
								presetAlias: 'element',
							},
						},
						// TODO: hide the queryParams and filter config under a "elementTypesOnly" field. [MR]
						search: {
							queryParams: {
								isElementType: true,
							},
						},
						pickableFilter: (docType) =>
							// Only pick elements:
							docType.isElement &&
							// Prevent picking the an already used element type:
							this.#filter &&
							this.#filter.find((x) => x.contentElementTypeKey === docType.unique) === undefined,
					},
					value: {
						selection: [],
					},
				};
			})
			.onSubmit((value) => {
				const selectedElementType = value.selection[0];

				if (selectedElementType) {
					this.dispatchEvent(new CustomEvent('create', { detail: { contentElementTypeKey: selectedElementType } }));
				}
			})
			.observeRouteBuilder((routeBuilder) => {
				const oldPath = this._pickerPath;
				this._pickerPath = routeBuilder({});
				this.requestUpdate('_pickerPath', oldPath);
			});
	}

	deleteItem(contentElementTypeKey: string) {
		this._value = this.value.filter((x) => x.contentElementTypeKey !== contentElementTypeKey);
		this.dispatchEvent(new UmbChangeEvent());
	}

	async #onRequestDelete(item: BlockType) {
		const store = await this.getContext(UMB_DOCUMENT_TYPE_ITEM_STORE_CONTEXT);
		if (!store) {
			return;
		}
		const contentType = store.getItems([item.contentElementTypeKey]);
		await umbConfirmModal(this, {
			color: 'danger',
			headline: `Remove ${contentType[0]?.name}?`,
			// TODO: Translations: [NL]
			content: 'Are you sure you want to remove this Block Type Configuration?',
			confirmLabel: 'Remove',
		});
		this.deleteItem(item.contentElementTypeKey);
	}

	override render() {
		return html`<div id="blocks">
			${repeat(this.value, (block) => block.contentElementTypeKey, this.#renderItem)} ${this.#renderButton()}
		</div>`;
	}

	#renderItem = (block: BlockType) => {
		return html`
			<umb-block-type-card
				.iconFile=${block.thumbnail}
				.iconColor=${block.iconColor}
				.backgroundColor=${block.backgroundColor}
				.href="${this.workspacePath}edit/${block.contentElementTypeKey}"
				.contentElementTypeKey=${block.contentElementTypeKey}>
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this.#onRequestDelete(block)} label="Remove block">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</umb-block-type-card>
		`;
	};

	#renderButton() {
		return this._pickerPath
			? html`
					<uui-button id="add-button" look="placeholder" href=${this._pickerPath} label="open">
						<uui-icon name="icon-add"></uui-icon>
						Add
					</uui-button>
				`
			: null;
	}

	static override styles = [
		css`
			div {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
				grid-template-rows: repeat(auto-fill, minmax(var(--umb-card-medium-min-width), 1fr));
			}

			[drag-placeholder] {
				opacity: 0.5;
			}

			#add-button {
				text-align: center;
				min-height: 150px;
				height: 100%;
			}

			uui-icon {
				display: block;
				margin: 0 auto;
			}

			uui-input {
				border: none;
				margin: var(--uui-size-space-6) 0 var(--uui-size-space-4);
			}

			uui-input:hover uui-button {
				opacity: 1;
			}
			uui-input uui-button {
				opacity: 0;
			}
		`,
	];
}

export default UmbInputBlockTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-input-block-type': UmbInputBlockTypeElement;
	}
}
