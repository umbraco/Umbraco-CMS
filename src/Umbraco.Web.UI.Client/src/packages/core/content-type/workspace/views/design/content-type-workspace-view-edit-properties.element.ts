import './content-type-workspace-view-edit-property.element.js';
import { UMB_CONTENT_TYPE_WORKSPACE_CONTEXT } from '../../content-type-workspace.context-token.js';
import type { UmbContentTypeWorkspacePropertyElement } from './content-type-workspace-view-edit-property.element.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbPropertyContainerTypes,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import {
	UmbContentTypePropertyStructureHelper,
	UMB_PROPERTY_SETTINGS_MODAL,
} from '@umbraco-cms/backoffice/content-type';
import { type UmbSorterConfig, UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { type UmbModalRouteBuilder, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

const SORTER_CONFIG: UmbSorterConfig<UmbPropertyTypeModel, UmbContentTypeWorkspacePropertyElement> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-umb-property-id');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.id;
	},
	identifier: 'document-type-property-sorter',
	itemSelector: 'umb-content-type-workspace-view-edit-property',
	//disabledItemSelector: '[inherited]',
	//TODO: Set the property list (sorter wrapper) to inherited, if its inherited
	// This is because we don't want to move local properties into an inherited group container.
	// Or maybe we do, but we still need to check if the group exists locally, if not, then it needs to be created before we move a property into it.
	// TODO: Fix bug where a local property turn into an inherited when moved to a new group container.
	containerSelector: '#property-list',
};

@customElement('umb-content-type-workspace-view-edit-properties')
export class UmbContentTypeWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#sorter = new UmbSorterController<UmbPropertyTypeModel, UmbContentTypeWorkspacePropertyElement>(this, {
		...SORTER_CONFIG,
		onChange: ({ model }) => {
			this._propertyStructure = model;
		},
		onEnd: ({ item }) => {
			if (this._containerId === undefined) {
				throw new Error('ContainerId is not set, we have not made a local duplicated of this container.');
				return;
			}
			console.log('containerId', this._containerId);
			/** Explanation: If the item is the first in list, we compare it to the item behind it to set a sortOrder.
			 * If it's not the first in list, we will compare to the item in before it, and check the following item to see if it caused overlapping sortOrder, then update
			 * the overlap if true, which may cause another overlap, so we loop through them till no more overlaps...
			 */
			const model = this._propertyStructure;
			const newIndex = model.findIndex((entry) => entry.id === item.id);

			// Doesn't exist in model
			if (newIndex === -1) return;

			// As origin we set prev sort order to -1, so if no other then our item will become 0
			let prevSortOrder = -1;

			// Not first in list
			if (newIndex > 0 && model.length > 0) {
				prevSortOrder = model[newIndex - 1].sortOrder;
			}

			// increase the prevSortOrder and use it for the moved item,
			this._propertyStructureHelper.partialUpdateProperty(item.id, {
				sortOrder: ++prevSortOrder,
			});

			// Adjust everyone right after, meaning until there is a gap between the sortOrders:
			let i = newIndex + 1;
			let entry: UmbPropertyTypeModel | undefined;
			// As long as there is an item with the index & the sortOrder is less or equal to the prevSortOrder, we will update the sortOrder:
			while ((entry = model[i]) !== undefined && entry.sortOrder <= prevSortOrder) {
				// Increase the prevSortOrder and use it for the item:
				this._propertyStructureHelper.partialUpdateProperty(entry.id, {
					sortOrder: ++prevSortOrder,
				});

				i++;
			}
		},
	});

	private _containerId: string | null | undefined;

	@property({ type: String, attribute: 'container-id', reflect: false })
	public get containerId(): string | null | undefined {
		return this._containerId;
	}
	public set containerId(value: string | null | undefined) {
		if (value === this._containerId) return;
		const oldValue = this._containerId;
		this._containerId = value;
		this.#addPropertyModal.setUniquePathValue('container-id', value === null ? 'root' : value);
		this.requestUpdate('containerId', oldValue);
	}

	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerName(): string | undefined {
		return this._propertyStructureHelper.getContainerName();
	}
	public set containerName(value: string | undefined) {
		this._propertyStructureHelper.setContainerName(value);
	}

	@property({ type: String, attribute: 'container-type', reflect: false })
	public get containerType(): UmbPropertyContainerTypes | undefined {
		return this._propertyStructureHelper.getContainerType();
	}
	public set containerType(value: UmbPropertyContainerTypes | undefined) {
		this._propertyStructureHelper.setContainerType(value);
	}

	#addPropertyModal: UmbModalRouteRegistrationController;

	_propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

	@state()
	_propertyStructure: Array<UmbPropertyTypeModel> = [];

	@state()
	_ownerDocumentType?: UmbContentTypeModel;

	@state()
	protected _modalRouteBuilderNewProperty?: UmbModalRouteBuilder;

	@state()
	_sortModeActive?: boolean;

	constructor() {
		super();

		this.#sorter.disable();

		this.consumeContext(UMB_CONTENT_TYPE_WORKSPACE_CONTEXT, async (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager(workspaceContext.structure);

			this.observe(
				workspaceContext.isSorting,
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
			const docTypeObservable = workspaceContext.structure.ownerContentType;
			this.observe(
				docTypeObservable,
				(document) => {
					this._ownerDocumentType = document;
				},
				'observeOwnerDocumentType',
			);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
			this.#sorter.setModel(this._propertyStructure);
		});

		// Note: Route for adding a new property
		this.#addPropertyModal = new UmbModalRouteRegistrationController(this, UMB_PROPERTY_SETTINGS_MODAL)
			.addUniquePaths(['container-id'])
			.addAdditionalPath('add-property/:sortOrder')
			.onSetup(async (params) => {
				if (!this._ownerDocumentType || !this._containerId) return false;

				const propertyData = await this._propertyStructureHelper.createPropertyScaffold(this._containerId);
				if (propertyData === undefined) return false;
				if (params.sortOrder !== undefined) {
					let sortOrderInt = parseInt(params.sortOrder, 10);
					if (sortOrderInt === -1) {
						// Find the highest sortOrder and add 1 to it:
						sortOrderInt = Math.max(...this._propertyStructure.map((x) => x.sortOrder), -1);
					}
					propertyData.sortOrder = sortOrderInt + 1;
				}
				return { data: { documentTypeId: this._ownerDocumentType.unique }, value: propertyData };
			})
			.onSubmit(async (value) => {
				if (!this._ownerDocumentType) return false;
				// TODO: The model requires a data-type to be set, we cheat currently. But this should be re-though when we implement validation(As we most likely will have to com up with partial models for the runtime model.) [NL]
				this._propertyStructureHelper.insertProperty(value as UmbPropertyTypeModel);
				return true;
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRouteBuilderNewProperty = routeBuilder;
			});
	}

	render() {
		return this._ownerDocumentType
			? html`
					<div id="property-list" ?sort-mode-active=${this._sortModeActive}>
						${repeat(
							this._propertyStructure,
							(property) => property.id,
							(property) => {
								return html`
									<umb-content-type-workspace-view-edit-property
										data-umb-property-id=${property.id}
										owner-document-type-id=${ifDefined(this._ownerDocumentType!.unique)}
										owner-document-type-name=${ifDefined(this._ownerDocumentType!.name)}
										?inherited=${property.container?.id !== this.containerId}
										?sort-mode-active=${this._sortModeActive}
										.propertyStructureHelper=${this._propertyStructureHelper}
										.property=${property}>
									</umb-content-type-workspace-view-edit-property>
								`;
							},
						)}
					</div>

					${!this._sortModeActive
						? html`<uui-button
								label=${this.localize.term('contentTypeEditor_addProperty')}
								id="add"
								look="placeholder"
								href=${ifDefined(this._modalRouteBuilderNewProperty?.({ sortOrder: -1 }))}>
								<umb-localize key="contentTypeEditor_addProperty">Add property</umb-localize>
							</uui-button> `
						: ''}
				`
			: '';
	}

	static styles = [
		UmbTextStyles,
		css`
			#add {
				width: 100%;
			}

			#property-list[sort-mode-active]:not(:has(umb-content-type-workspace-view-edit-property)) {
				/* Some height so that the sorter can target the area if the group is empty*/
				min-height: var(--uui-size-layout-1);
			}
		`,
	];
}

export default UmbContentTypeWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-type-workspace-view-edit-properties': UmbContentTypeWorkspaceViewEditPropertiesElement;
	}
}
