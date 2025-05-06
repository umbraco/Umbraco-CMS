import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '../../content-type-workspace.context-token.js';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '../../../types.js';
import { UmbContentTypePropertyStructureHelper } from '../../../structure/index.js';
import type { UmbContentTypeDesignEditorPropertyElement } from './content-type-design-editor-property.element.js';
import { UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT } from './content-type-design-editor.context-token.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	property,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { type UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import {
	UMB_CREATE_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_PROPERTY_TYPE_WORKSPACE_MODAL,
} from '@umbraco-cms/backoffice/property-type';

import './content-type-design-editor-property.element.js';

const SORTER_CONFIG: UmbSorterConfig<UmbPropertyTypeModel, UmbContentTypeDesignEditorPropertyElement> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-umb-property-id');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.unique;
	},
	identifier: 'content-type-property-sorter',
	itemSelector: 'umb-content-type-design-editor-property',
	//disabledItemSelector: '[inherited]',
	//TODO: Set the property list (sorter wrapper) to inherited, if its inherited
	// This is because we don't want to move local properties into an inherited group container.
	// Or maybe we do, but we still need to check if the group exists locally, if not, then it needs to be created before we move a property into it.
	// TODO: Fix bug where a local property turn into an inherited when moved to a new group container.
	containerSelector: '#property-list',
};

@customElement('umb-content-type-design-editor-properties')
export class UmbContentTypeDesignEditorPropertiesElement extends UmbLitElement {
	#sorter = new UmbSorterController<UmbPropertyTypeModel, UmbContentTypeDesignEditorPropertyElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this._properties = model;
		},
		onContainerChange: ({ item }) => {
			if (this._containerId === undefined) {
				throw new Error('ContainerId is not set');
			}
			this.#propertyStructureHelper.partialUpdateProperty(item.unique, {
				container: this._containerId ? { id: this._containerId } : null,
			});
		},
		onEnd: ({ item }) => {
			if (this._containerId === undefined) {
				throw new Error('ContainerId is not set.');
			}
			/**
			 * Explanation: If the item is the first in list, we compare it to the item behind it to set a sortOrder.
			 * If it's not the first in list, we will compare to the item in before it, and check the following item to see if it caused overlapping sortOrder, then update
			 * the overlap if true, which may cause another overlap, so we loop through them till no more overlaps...
			 */
			const model = this._properties;
			const newIndex = model.findIndex((entry) => entry.unique === item.unique);

			// Doesn't exist in model
			if (newIndex === -1) return;

			// As origin we set prev sort order to -1, so if no other then our item will become 0
			let prevSortOrder = -1;

			// Not first in list
			if (newIndex > 0 && model.length > 0) {
				prevSortOrder = model[newIndex - 1].sortOrder;
			}

			// increase the prevSortOrder and use it for the moved item,
			this.#propertyStructureHelper.partialUpdateProperty(item.unique, {
				sortOrder: ++prevSortOrder,
			});

			// Adjust everyone right after, meaning until there is a gap between the sortOrders:
			let i = newIndex + 1;
			let entry: UmbPropertyTypeModel | undefined;
			// As long as there is an item with the index & the sortOrder is less or equal to the prevSortOrder, we will update the sortOrder:
			while ((entry = model[i]) !== undefined && entry.sortOrder <= prevSortOrder) {
				// Increase the prevSortOrder and use it for the item:
				this.#propertyStructureHelper.partialUpdateProperty(entry.unique, {
					sortOrder: ++prevSortOrder,
				});

				i++;
			}
		},
		onRequestDrop: async ({ unique }) => {
			const context = await this.getContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT);
			if (!context) {
				throw new Error('Could not get Workspace Context');
			}
			return context.structure.getOwnerPropertyById(unique);
		},
		requestExternalRemove: async ({ item }) => {
			const context = await this.getContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT);
			if (!context) {
				throw new Error('Could not get Workspace Context');
			}
			return await context.structure.removeProperty(null, item.unique).then(
				() => true,
				() => false,
			);
		},
		requestExternalInsert: async ({ item }) => {
			const context = await this.getContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT);
			if (!context) {
				throw new Error('Could not get Workspace Context');
			}
			const parent = this._containerId ? { id: this._containerId } : null;
			const updatedItem = { ...item, parent };
			return await context.structure.insertProperty(null, updatedItem).then(
				() => true,
				() => false,
			);
		},
	});

	private _containerId: string | null | undefined;

	@property({ type: String, attribute: 'container-id', reflect: false })
	public get containerId(): string | null | undefined {
		return this._containerId;
	}
	public set containerId(value: string | null | undefined) {
		if (value === this._containerId) return;
		this._containerId = value;
		this.createPropertyTypeWorkspaceRoutes();
		this.#propertyStructureHelper.setContainerId(value);
		this.#addPropertyModal?.setUniquePathValue('container-id', value === null ? 'root' : value);
		this.#editPropertyModal?.setUniquePathValue('container-id', value === null ? 'root' : value);
	}

	#addPropertyModal?: UmbModalRouteRegistrationController<
		typeof UMB_PROPERTY_TYPE_WORKSPACE_MODAL.DATA,
		typeof UMB_PROPERTY_TYPE_WORKSPACE_MODAL.VALUE
	>;
	#editPropertyModal?: UmbModalRouteRegistrationController<
		typeof UMB_PROPERTY_TYPE_WORKSPACE_MODAL.DATA,
		typeof UMB_PROPERTY_TYPE_WORKSPACE_MODAL.VALUE
	>;

	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

	@property({ attribute: false })
	editContentTypePath?: string;

	@state()
	private _properties: Array<UmbPropertyTypeModel> = [];

	@state()
	private _ownerContentTypeUnique?: string;

	@state()
	private _ownerContentTypeVariesByCulture?: boolean;

	@state()
	private _ownerContentTypeVariesBySegment?: boolean;

	@state()
	private _newPropertyPath?: string;

	@state()
	private _editPropertyTypePath?: string;

	@state()
	private _sortModeActive?: boolean;

	constructor() {
		super();

		this.#sorter.disable();

		this.consumeContext(UMB_CONTENT_TYPE_DESIGN_EDITOR_CONTEXT, (context) => {
			this.observe(
				context?.isSorting,
				(isSorting) => {
					this._sortModeActive = isSorting;
					if (isSorting) {
						this.#sorter.enable();
					} else {
						this.#sorter.disable();
					}
				},
				'_observeIsSorting',
			);
		});

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, async (workspaceContext) => {
			if (workspaceContext) {
				this.#propertyStructureHelper.setStructureManager(workspaceContext.structure);
			}

			this._ownerContentTypeUnique = workspaceContext?.structure.getOwnerContentTypeUnique();
			this.createPropertyTypeWorkspaceRoutes();

			this.observe(
				workspaceContext?.variesByCulture,
				(variesByCulture) => {
					this._ownerContentTypeVariesByCulture = variesByCulture;
				},
				'observeOwnerVariesByCulture',
			);
			this.observe(
				workspaceContext?.variesBySegment,
				(variesBySegment) => {
					this._ownerContentTypeVariesBySegment = variesBySegment;
				},
				'observeOwnerVariesBySegment',
			);
		});
		this.observe(this.#propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._properties = propertyStructure;
			this.#sorter.setModel(this._properties);
		});
	}

	createPropertyTypeWorkspaceRoutes() {
		if (!this._ownerContentTypeUnique || this._containerId === undefined) return;

		// Note: Route for adding a new property
		this.#addPropertyModal?.destroy();
		this.#addPropertyModal = new UmbModalRouteRegistrationController(
			this,
			UMB_PROPERTY_TYPE_WORKSPACE_MODAL,
			'addPropertyModal',
		)
			.addUniquePaths(['container-id'])
			.addAdditionalPath('add-property/:sortOrder')
			.onSetup(async (params) => {
				// TODO: Make a onInit promise, that can be awaited here.
				if (!this._ownerContentTypeUnique || this._containerId === undefined) return false;

				const preset: Partial<UmbPropertyTypeModel> = {};
				if (params.sortOrder !== undefined) {
					let sortOrderInt = parseInt(params.sortOrder);
					if (sortOrderInt === -1) {
						// Find the highest sortOrder and add 1 to it:
						sortOrderInt = Math.max(...this._properties.map((x) => x.sortOrder), -1) + 1;
					}
					preset.sortOrder = sortOrderInt;
				}
				return { data: { contentTypeUnique: this._ownerContentTypeUnique, preset: preset } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._newPropertyPath =
					routeBuilder({ sortOrder: '-1' }) +
					UMB_CREATE_PROPERTY_TYPE_WORKSPACE_PATH_PATTERN.generateLocal({
						containerUnique: this._containerId!,
					});
			});

		if (this._containerId !== undefined) {
			this.#addPropertyModal?.setUniquePathValue(
				'container-id',
				this._containerId === null ? 'root' : this._containerId,
			);
		}

		this.#editPropertyModal?.destroy();
		this.#editPropertyModal = new UmbModalRouteRegistrationController(
			this,
			UMB_PROPERTY_TYPE_WORKSPACE_MODAL,
			'editPropertyModal',
		)
			.addUniquePaths(['container-id'])
			.addAdditionalPath('edit-property')
			.onSetup(async () => {
				if (!this._ownerContentTypeUnique || this._containerId === undefined) return false;
				return { data: { contentTypeUnique: this._ownerContentTypeUnique, preset: undefined } };
			})
			.observeRouteBuilder((routeBuilder) => {
				this._editPropertyTypePath = routeBuilder(null);
			});
		if (this._containerId !== undefined) {
			this.#editPropertyModal?.setUniquePathValue(
				'container-id',
				this._containerId === null ? 'root' : this._containerId,
			);
		}
	}

	override render() {
		return this._ownerContentTypeUnique
			? html`
					<div id="property-list" ?sort-mode-active=${this._sortModeActive}>
						${repeat(
							this._properties,
							(property) => property.unique,
							(property) => {
								return html`
									<umb-content-type-design-editor-property
										data-umb-property-id=${property.unique}
										data-mark="property-type:${property.name}"
										.editContentTypePath=${this.editContentTypePath}
										.editPropertyTypePath=${this._editPropertyTypePath}
										?sort-mode-active=${this._sortModeActive}
										.propertyStructureHelper=${this.#propertyStructureHelper}
										.property=${property}
										.ownerVariesByCulture=${this._ownerContentTypeVariesByCulture}
										.ownerVariesBySegment=${this._ownerContentTypeVariesBySegment}>
									</umb-content-type-design-editor-property>
								`;
							},
						)}
					</div>

					${when(
						!this._sortModeActive,
						() => html`
							<uui-button
								id="btn-add"
								href=${ifDefined(this._newPropertyPath)}
								label=${this.localize.term('contentTypeEditor_addProperty')}
								look="placeholder"></uui-button>
						`,
					)}
				`
			: '';
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: block;
			}
			#btn-add {
				width: 100%;
				--uui-button-height: var(--uui-size-14);
			}

			#property-list[sort-mode-active]:not(:has(umb-content-type-design-editor-property)) {
				/* Some height so that the sorter can target the area if the group is empty*/
				min-height: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbContentTypeDesignEditorPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-design-editor-properties': UmbContentTypeDesignEditorPropertiesElement;
	}
}
