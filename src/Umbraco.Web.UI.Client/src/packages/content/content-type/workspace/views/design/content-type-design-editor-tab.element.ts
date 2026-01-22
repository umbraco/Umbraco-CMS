import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '../../content-type-workspace.context-token.js';
import type {
	UmbContentTypeModel,
	UmbPropertyTypeContainerMergedModel,
	UmbPropertyTypeContainerModel,
} from '../../../types.js';
import { UmbContentTypeContainerStructureHelper } from '../../../structure/index.js';
import type { UmbContentTypeWorkspaceViewEditGroupElement } from './content-type-design-editor-group.element.js';
import { UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT } from './content-type-design-editor.context-token.js';
import { css, customElement, html, nothing, property, repeat, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UMB_WORKSPACE_MODAL } from '@umbraco-cms/backoffice/workspace';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';

import './content-type-design-editor-properties.element.js';
import './content-type-design-editor-group.element.js';

const SORTER_CONFIG: UmbSorterConfig<UmbPropertyTypeContainerMergedModel, UmbContentTypeWorkspaceViewEditGroupElement> =
	{
		getUniqueOfElement: (element) => element.group?.ownerId ?? element.group?.ids[0],
		getUniqueOfModel: (modelEntry) => modelEntry.ownerId ?? modelEntry.ids[0],
		// TODO: Make specific to the current owner document. [NL]
		identifier: 'content-type-container-sorter',
		itemSelector: 'umb-content-type-design-editor-group',
		handleSelector: '.drag-handle',
		disabledItemSelector: ':not([has-owner-container])', // Inherited attribute is set by the umb-content-type-design-editor-group.
		containerSelector: '.container-list',
	};

@customElement('umb-content-type-design-editor-tab')
export class UmbContentTypeDesignEditorTabElement extends UmbLitElement {
	#sorter = new UmbSorterController<UmbPropertyTypeContainerMergedModel, UmbContentTypeWorkspaceViewEditGroupElement>(
		this,
		{
			...SORTER_CONFIG,
			onChange: ({ model }) => {
				this._groups = model;
			},
			onContainerChange: async ({ item }) => {
				if (this.#containerId === undefined) {
					throw new Error('ContainerId is not set');
				}
				if (item.ownerId === undefined) {
					// This may be possible later, but for now this is not possible. [NL]
					throw new Error(
						'OwnerId is not set for the given container, we cannot move containers that are not owned by the current Document.',
					);
				}

				let containerId = this.#containerId;
				// ensure container is local:
				if (containerId !== null) {
					const structureManager = this.#groupStructureHelper.getStructureManager();
					if (!structureManager) {
						throw new Error('Could not get Structure Manager from Structure Helper');
					}
					const container = await structureManager.ensureContainerOf(
						containerId,
						structureManager.getOwnerContentType()!.unique,
					);
					containerId = container?.id || null;
				}

				this.#groupStructureHelper.partialUpdateContainer(item.ownerId, {
					parent: containerId ? { id: containerId } : null,
				});
			},
			onEnd: ({ item }) => {
				if (item.ownerId === undefined) {
					// This may be possible later, but for now this is not possible. [NL]
					throw new Error(
						'OwnerId is not set for the given container, we cannot move containers that are not owned by the current Document.',
					);
				}
				/**
				 * Explanation: If the item is the first in list, we compare it to the item behind it to set a sortOrder.
				 * If it's not the first in list, we will compare to the item in before it, and check the following item to see if it caused overlapping sortOrder, then update
				 * the overlap if true, which may cause another overlap, so we loop through them till no more overlaps...
				 */
				const model = this._groups;
				const newIndex = model.findIndex((entry) => entry.ownerId === item.ownerId);

				// Doesn't exist in model
				if (newIndex === -1) return;

				// As origin we set prev sort order to -1, so if no other then our item will become 0
				let prevSortOrder = -1;

				// Not first in list
				if (newIndex > 0 && model.length > 0) {
					prevSortOrder = model[newIndex - 1].sortOrder;
				}

				const ownerId = item.ownerId;

				if (ownerId === undefined) {
					// This may be possible later, but for now this is not possible. [NL]
					throw new Error(
						'OwnerId is not set for the given container, we cannot move containers that are not owned by the current Document.',
					);
				}

				// increase the prevSortOrder and use it for the moved item,
				this.#groupStructureHelper.partialUpdateContainer(ownerId, {
					sortOrder: ++prevSortOrder,
				});

				// Adjust everyone right after, meaning until there is a gap between the sortOrders:
				let i = newIndex + 1;
				let entry: UmbPropertyTypeContainerMergedModel | undefined;
				// As long as there is an item with the index & the sortOrder is less or equal to the prevSortOrder, we will update the sortOrder:
				while ((entry = model[i]) !== undefined && entry.sortOrder <= prevSortOrder) {
					// Only updated owned containers:
					if (entry.ownerId) {
						// Increase the prevSortOrder and use it for the item:
						this.#groupStructureHelper.partialUpdateContainer(entry.ownerId, {
							sortOrder: ++prevSortOrder,
						});
					}
					i++;
				}
			},
			onRequestDrop: async ({ unique }) => {
				const context = this.#contentTypeWorkspaceContext;
				if (!context) {
					throw new Error('Could not get Workspace Context');
				}
				const result = context.structure.getMergedContainerById(unique) as
					| UmbPropertyTypeContainerMergedModel
					| undefined;
				return result;
			},
			requestExternalRemove: async ({ item }) => {
				const context = this.#contentTypeWorkspaceContext;
				if (!context) {
					throw new Error('Could not get Workspace Context');
				}
				return await context.structure.removeContainer(null, item.ownerId, { preventRemovingProperties: true }).then(
					() => true,
					() => false,
				);
			},
			requestExternalInsert: async ({ item }) => {
				const context = this.#contentTypeWorkspaceContext;
				if (!context) {
					throw new Error('Could not get Workspace Context');
				}
				if (item.ownerId === undefined) {
					// This may be possible later, but for now this is not possible. [NL]
					throw new Error('OwnerId is not set, we cannot move containers that are not owned by the current Document.');
				}
				const parent = this.#containerId ? { id: this.#containerId } : null;
				const containerModelItem: UmbPropertyTypeContainerModel = {
					id: item.ownerId,
					name: item.name,
					sortOrder: item.sortOrder,
					type: item.type,
					parent,
				};
				return await context.structure.insertContainer(null, containerModelItem).then(
					() => true,
					() => false,
				);
			},
		},
	);

	#workspaceModal?: UmbModalRouteRegistrationController<
		typeof UMB_WORKSPACE_MODAL.DATA,
		typeof UMB_WORKSPACE_MODAL.VALUE
	>;
	#containerId?: string | null;

	@property({ type: String })
	public get containerId(): string | null | undefined {
		return this.#containerId;
	}
	public set containerId(value: string | null | undefined) {
		const oldValue = this.#containerId;
		if (value === this.#containerId) return;
		this.#containerId = value;
		this.#groupStructureHelper.setContainerId(value);
		this.requestUpdate('containerId', oldValue);
	}

	@state()
	private _groups: Array<UmbPropertyTypeContainerMergedModel> = [];

	@state()
	private _hasProperties = false;

	@state()
	private _sortModeActive?: boolean;

	@state()
	private _editContentTypePath?: string;

	#groupStructureHelper = new UmbContentTypeContainerStructureHelper<UmbContentTypeModel>(this);
	#contentTypeWorkspaceContext?: typeof UMB_CONTENT_TYPE_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, (context) => {
			this.#contentTypeWorkspaceContext = context;
			this.#groupStructureHelper.setStructureManager(context?.structure);

			const entityType = context?.getEntityType();

			this.#workspaceModal?.destroy();
			this.#workspaceModal = new UmbModalRouteRegistrationController(this, UMB_WORKSPACE_MODAL)
				.addAdditionalPath(entityType ?? 'unknown')
				.onSetup(async () => {
					return { data: { entityType: entityType, preset: {} } };
				})
				.observeRouteBuilder((routeBuilder) => {
					this._editContentTypePath = routeBuilder({});
				});
		});

		this.consumeContext(UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT, (context) => {
			this.observe(
				context?.isSorting,
				(isSorting) => {
					this._sortModeActive = isSorting;
				},
				'_observeIsSorting',
			);
		});

		this.observe(
			this.#groupStructureHelper.childContainers,
			(groups) => {
				this._groups = groups;
				this.#sorter.setModel(this._groups);
			},
			null,
		);

		this.observe(
			this.#groupStructureHelper.hasProperties,
			(hasProperties) => {
				this._hasProperties = hasProperties;
				this.requestUpdate('_hasProperties');
			},
			null,
		);
	}

	#onAddGroup = () => {
		// Idea, maybe we can gather the sortOrder from the last group rendered and add 1 to it?
		const len = this._groups.length;
		const sortOrder = len === 0 ? 0 : this._groups[len - 1].sortOrder + 1;
		this.#groupStructureHelper.addContainer(this.#containerId, sortOrder);
	};

	override render() {
		return html`
			${
				this.#containerId
					? html`
							<uui-box class="${this._hasProperties ? '' : 'opaque'}">
								<umb-content-type-design-editor-properties
									.containerId=${this.containerId}
									.editContentTypePath=${this._editContentTypePath}></umb-content-type-design-editor-properties>
							</uui-box>
						`
					: nothing
			}

				<div class="container-list" ?sort-mode-active=${this._sortModeActive}>
					${repeat(
						this._groups,
						(group) => group.ids[0],
						(group) => html`
							<umb-content-type-design-editor-group
								?sort-mode-active=${this._sortModeActive}
								.editContentTypePath=${this._editContentTypePath}
								.group=${group}
								.groupStructureHelper=${this.#groupStructureHelper as any}
								data-umb-group-id=${group.ownerId ?? group.ids[0]}
								data-mark="group:${group.name}">
							</umb-content-type-design-editor-group>
						`,
					)}
				</div>
				${this.#renderAddGroupButton()}
			</div>
		`;
	}

	#renderAddGroupButton() {
		if (this._sortModeActive) return;
		return html`
			<uui-button
				id="btn-add"
				label=${this.localize.term('contentTypeEditor_addGroup')}
				look="placeholder"
				@click=${this.#onAddGroup}></uui-button>
		`;
	}

	static override styles = [
		css`
			#btn-add {
				width: 100%;
				--uui-button-height: var(--uui-size-24);
			}

			uui-box,
			umb-content-type-design-editor-group {
				margin-bottom: var(--uui-size-layout-1);
			}
			uui-box.opaque {
				background-color: transparent;
				border-color: transparent;
			}

			.container-list {
				display: grid;
				gap: 10px;
				align-content: start;
			}

			/* Ensure the container-list has some height when its empty so groups can be dropped into it.*/
			.container-list {
				margin-top: calc(var(--uui-size-layout-1) * -1);
				padding-top: var(--uui-size-layout-1);
			}

			.container-list #convert-to-tab {
				margin-bottom: var(--uui-size-layout-1);
				display: flex;
			}

			.container-list[sort-mode-active] {
				min-height: 100px;
			}
		`,
	];
}

export default UmbContentTypeDesignEditorTabElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-design-editor-tab': UmbContentTypeDesignEditorTabElement;
	}
}
