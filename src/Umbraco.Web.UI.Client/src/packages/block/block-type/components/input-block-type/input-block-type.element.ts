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
import { BlockGridGroupConfigration } from '@umbraco-cms/backoffice/block';
import { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_PROPERTY_DATASET_CONTEXT, UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';

@customElement('umb-input-block-type')
export class UmbInputBlockTypeElement<
	BlockType extends UmbBlockTypeWithGroupKey = UmbBlockTypeBase,
> extends UmbLitElement {
	//
	@property({ type: Array, attribute: false })
	public get value() {
		return this._items;
	}
	public set value(items) {
		this._items = items ?? [];
		this.#mapValues();
	}

	@property({ type: String, attribute: 'entity-type' })
	public get entityType() {
		return this.#entityType;
	}
	public set entityType(entityType) {
		this.#entityType = entityType;

		this.#blockTypeWorkspaceModalRegistration?.destroy();

		if (entityType) {
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

	#groups: Array<BlockGridGroupConfigration> = [];
	@property({ type: Array })
	public get groups(): Array<BlockGridGroupConfigration> {
		return this.#groups;
	}
	public set groups(groups: Array<BlockGridGroupConfigration>) {
		this.#groups = groups ?? [];
		this.#mapValues();
	}

	#mapValues() {
		const valuesWithNoGroup = this.value.filter(
			// Look for values without a group, or with a group that is non existent.
			(value) => !value.groupKey || this.#groups.find((group) => group.key !== value.groupKey),
		);

		const valuesWithGroup = this.#groups.map((group) => {
			return { name: group.name, key: group.key, blocks: this.value.filter((value) => value.groupKey === group.key) };
		});

		this._mappedGroups = [{ key: '', name: '', blocks: valuesWithNoGroup }, ...valuesWithGroup];
	}

	@state()
	private _items: Array<BlockType> = [];

	@state()
	private _mappedGroups: Array<BlockGridGroupConfigration & { blocks: Array<BlockType> }> = [];

	@state()
	private _workspacePath?: string;

	#blockTypeWorkspaceModalRegistration?: UmbModalRouteRegistrationController<
		typeof UMB_WORKSPACE_MODAL.DATA,
		typeof UMB_WORKSPACE_MODAL.VALUE
	>;

	#context?: UmbPropertyDatasetContext;

	constructor() {
		super();
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, async (instance) => {
			this.#context = instance;
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
							this._items.find((x) => x.contentElementTypeKey === docType.id) === undefined,
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

	#renameGroup(e: UUIInputEvent, key: string) {
		if (!key) return;
		const groupName = e.target.value as string;
		const groups = this.groups.map((group) => (group.key === key ? { ...group, name: groupName } : group));

		this.#context?.setPropertyValue('blockGroups', groups);
	}

	#deleteGroup(key: string) {
		this.#context?.setPropertyValue(
			'blockGroups',
			this.groups.filter((group) => group.key !== key),
		);
		this.value = this.value.filter((block) => block.groupKey !== key);
		this.dispatchEvent(new UmbChangeEvent());
		this.#mapValues();
	}

	render() {
		return html`${repeat(
			this._mappedGroups,
			(group) => group.key + group.blocks,
			(group) =>
				html` ${group.key
						? html`<uui-input
								auto-width
								.value=${group.name}
								label="Group"
								@change=${(e: UUIInputEvent) => this.#renameGroup(e, group.key)}>
								<uui-button compact slot="append" label="delete" @click=${() => this.#deleteGroup(group.key)}>
									<uui-icon name="icon-trash"></uui-icon>
								</uui-button>
						  </uui-input>`
						: ''}
					<div>
						${repeat(
							group.blocks,
							(block) => block.contentElementTypeKey,
							(block) =>
								html`<umb-block-type-card
									.workspacePath=${this._workspacePath}
									.key=${block.contentElementTypeKey}
									@delete=${() => this.deleteItem(block.contentElementTypeKey)}>
								</umb-block-type-card>`,
						)}
						${this.#renderButton()}
					</div>`,
		)}`;
	}

	#renderButton() {
		return html`
			<uui-button id="add-button" look="placeholder" @click=${() => this.create()} label="open">
				<uui-icon name="icon-add"></uui-icon>
				Add
			</uui-button>
		`;
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
