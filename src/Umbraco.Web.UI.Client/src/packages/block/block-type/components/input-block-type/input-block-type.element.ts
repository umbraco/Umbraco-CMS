import { UmbBlockTypeBase, UmbBlockTypeGroup, UmbBlockTypeWithGroupKey } from '../../types.js';
import {
	UMB_DOCUMENT_TYPE_PICKER_MODAL,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_WORKSPACE_MODAL,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';
import '../block-type-card/index.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbChangeEvent } from '@umbraco-cms/backoffice/event';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_PROPERTY_DATASET_CONTEXT, UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UmbBlockGridType } from '@umbraco-cms/backoffice/block';

@customElement('umb-input-block-type')
export class UmbInputBlockTypeElement<
	BlockType extends UmbBlockTypeWithGroupKey = UmbBlockTypeBase,
> extends UmbLitElement {
	@property({ type: Array, attribute: false })
	public get value() {
		return this._items;
	}
	public set value(items) {
		this._items = items ?? [];
	}

	@property()
	groupKey?: string;

	@property()
	groupName?: string;

	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this.#entityType;
	}
	public set entityType(entityType) {
		this.#entityType = entityType;

		this.#blockTypeWorkspaceModalRegistration?.destroy();

		if (entityType) {
			// TODO: Make specific modal token that requires data.
			this.#blockTypeWorkspaceModalRegistration = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
				.addAdditionalPath(entityType)
				.onSetup(() => {
					return { data: { entityType: entityType, preset: {} }, modal: { size: 'large' } };
				})
				.observeRouteBuilder((routeBuilder) => {
					const newpath = routeBuilder({});
					this._workspacePath = newpath;
				});
		}
	}
	#entityType?: string;

	@state()
	private _items: Array<BlockType> = [];

	@state()
	private _workspacePath?: string;

	#blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
		typeof UMB_WORKSPACE_MODAL.DATA,
		typeof UMB_WORKSPACE_MODAL.VALUE
	>;

	#datasetContext?: UmbPropertyDatasetContext;
	#blockGroups: Array<UmbBlockTypeGroup> = [];
	#filter?: any;

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#datasetContext = instance;
			this.observe(await this.#datasetContext?.propertyValueByAlias('blockGroups'), (value) => {
				this.#blockGroups = value as Array<UmbBlockTypeGroup>;
			});
			this.observe(await this.#datasetContext?.propertyValueByAlias('blocks'), (value) => {
				this.#filter = value as Array<UmbBlockGridType>;
			});
		});
	}

	create() {
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, async (modalManager) => {
			if (modalManager) {
				// TODO: Make as mode for the Picker Modal, so the click to select immediately submits the modal(And in that mode we do not want to see a Submit button).
				const modalContext = modalManager.open(UMB_DOCUMENT_TYPE_PICKER_MODAL, {
					data: {
						hideTreeRoot: true,
						multiple: false,
						pickableFilter: (docType) =>
							// Only pick elements:
							docType.isElement &&
							// Prevent picking the an already used element type:
							this.#filter.find((x: UmbBlockGridType) => x.contentElementTypeKey === docType.id) === undefined,
					},
				});

				const modalValue = await modalContext?.onSubmit();
				const selectedElementType = modalValue.selection[0];
				if (selectedElementType) {
					this.#blockTypeWorkspaceModalRegistration?.open({}, 'create/' + selectedElementType);
				}
			}
		});

		// No need to fire a change event, as all changes are made directly to the property, via context api.
	}

	deleteItem(contentElementTypeKey: string) {
		this.value = this._items.filter((x) => x.contentElementTypeKey !== contentElementTypeKey);
		this.dispatchEvent(new UmbChangeEvent());
	}

	protected getFormElement() {
		return undefined;
	}

	render() {
		return html`${this.#renderGroupInput()}
			<div>
				${repeat(this.value, (block) => block.contentElementTypeKey, this.#renderItem)} ${this.#renderButton()}
			</div>`;
	}

	#renderItem = (item: BlockType) => {
		return html`
			<umb-block-type-card
				.workspacePath=${this._workspacePath}
				.key=${item.contentElementTypeKey}
				@delete=${() => this.deleteItem(item.contentElementTypeKey)}>
			</umb-block-type-card>
		`;
	};

	#renderButton() {
		return html`
			<uui-button id="add-button" look="placeholder" @click=${() => this.create()} label="open">
				<uui-icon name="icon-add"></uui-icon>
				Add
			</uui-button>
		`;
	}

	//Group renders (if exists)

	#renderGroupInput() {
		if (!this.groupKey) return;
		return html`<uui-input auto-width label="Group" .value=${this.groupName} @change=${this.#changeGroupName}>
			<uui-button compact slot="append" label="delete" @click=${this.#deleteGroup}>
				<uui-icon name="icon-trash"></uui-icon>
			</uui-button>
		</uui-input>`;
	}

	#changeGroupName(e: UUIInputEvent) {
		if (!this.groupKey) return;

		const groupName = e.target.value as string;
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this.#blockGroups.map((group) => ({ ...group, groupName: this.groupKey === group.key ? groupName : group.name })),
		);
	}

	#deleteGroup() {
		if (!this.groupKey) return;
		this.#datasetContext?.setPropertyValue(
			'blockGroups',
			this.#blockGroups.filter((group) => group.key !== this.groupKey),
		);
		this.value = this._items.filter((block) => block.groupKey !== this.groupKey);
		this.dispatchEvent(new UmbChangeEvent());
	}

	static styles = [
		css`
			div {
				display: grid;
				gap: var(--uui-size-space-3);
				grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
				grid-template-rows: repeat(auto-fill, minmax(160px, 1fr));
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
