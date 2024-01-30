import type { UmbMediaTypeWorkspaceContext } from '../../media-type-workspace.context.js';
import './media-type-workspace-view-edit-property.element.js';
import type { UmbMediaTypeDetailModel } from '../../../types.js';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyContainerTypes, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbSorterController } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_PROPERTY_SETTINGS_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';

const SORTER_CONFIG: UmbSorterConfig<UmbPropertyTypeModel> = {
	compareElementToModel: (element: HTMLElement, model: UmbPropertyTypeModel) => {
		return element.getAttribute('data-umb-property-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: UmbPropertyTypeModel) => {
		return container.querySelector('[data-umb-property-id=' + modelEntry.id + ']');
	},
	identifier: 'content-type-property-sorter',
	itemSelector: '[data-umb-property-id]',
	disabledItemSelector: '[inherited]',
	containerSelector: '#property-list',
};

@customElement('umb-media-type-workspace-view-edit-properties')
export class UmbMediaTypeWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#propertySorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		performItemInsert: (args) => {
			let sortOrder = 0;
			if (this._propertyStructure.length > 0) {
				if (args.newIndex === 0) {
					sortOrder = (this._propertyStructure[0].sortOrder ?? 0) - 1;
				} else {
					sortOrder =
						(this._propertyStructure[Math.min(args.newIndex, this._propertyStructure.length - 1)].sortOrder ?? 0) + 1;
				}
			}
			return this._propertyStructureHelper.insertProperty(args.item, sortOrder);
		},
		performItemRemove: (args) => {
			return this._propertyStructureHelper.removeProperty(args.item.id!);
		},
		performItemMove: (args) => {
			this._propertyStructureHelper.removeProperty(args.item.id!);
			let sortOrder = 0;
			if (this._propertyStructure.length > 0) {
				if (args.newIndex === 0) {
					sortOrder = (this._propertyStructure[0].sortOrder ?? 0) - 1;
				} else {
					sortOrder =
						(this._propertyStructure[Math.min(args.newIndex, this._propertyStructure.length - 1)].sortOrder ?? 0) + 1;
				}
			}
			return this._propertyStructureHelper.insertProperty(args.item, sortOrder);
		},
	});

	private _containerId: string | undefined;

	@property({ type: String, attribute: 'container-id', reflect: false })
	public get containerId(): string | undefined {
		return this._containerId;
	}
	public set containerId(value: string | undefined) {
		if (value === this._containerId) return;
		const oldValue = this._containerId;
		this._containerId = value;
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

	_propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbMediaTypeDetailModel>(this);

	@state()
	_propertyStructure: Array<UmbPropertyTypeModel> = [];

	@state()
	_ownerMediaTypes?: UmbMediaTypeDetailModel[];

	@state()
	protected _modalRouteNewProperty?: string;

	@state()
	_sortModeActive?: boolean;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager((workspaceContext as UmbMediaTypeWorkspaceContext).structure);
			this.observe(
				(workspaceContext as UmbMediaTypeWorkspaceContext).isSorting,
				(isSorting) => {
					this._sortModeActive = isSorting;
					this.#setModel(isSorting);
				},
				'_observeIsSorting',
			);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
		});

		// Note: Route for adding a new property
		new UmbModalRouteRegistrationController(this, UMB_PROPERTY_SETTINGS_MODAL)
			.addAdditionalPath('new-property')
			.onSetup(async () => {
				const mediaTypeId = this._ownerMediaTypes?.find(
					(types) => types.containers?.find((containers) => containers.id === this.containerId),
				)?.unique;
				if (mediaTypeId === undefined) return false;
				const propertyData = await this._propertyStructureHelper.createPropertyScaffold(this._containerId);
				if (propertyData === undefined) return false;
				return { data: { documentTypeId: mediaTypeId }, value: propertyData }; //TODO: Should we have a separate modal for mediaTypes?
			})
			.onSubmit((value) => {
				this.#addProperty(value);
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRouteNewProperty = routeBuilder(null);
			});
	}

	#setModel(isSorting?: boolean) {
		if (isSorting) {
			this.#propertySorter.setModel(this._propertyStructure);
		} else {
			this.#propertySorter.setModel([]);
		}
	}

	connectedCallback(): void {
		super.connectedCallback();
		const mediaTypes = this._propertyStructureHelper.ownerDocumentTypes; //TODO: Should we have a separate propertyStructureHelper for mediaTypes?
		if (!mediaTypes) return;
		this.observe(
			mediaTypes,
			(medias) => {
				this._ownerMediaTypes = medias;
			},
			'observeOwnerMediaTypes',
		);
	}

	async #addProperty(propertyData: UmbPropertyTypeModel) {
		const propertyPlaceholder = await this._propertyStructureHelper.addProperty(this._containerId);
		if (!propertyPlaceholder) return;

		this._propertyStructureHelper.partialUpdateProperty(propertyPlaceholder.id, propertyData);
	}

	render() {
		return html`<div id="property-list">
				${repeat(
					this._propertyStructure,
					(property) => property.id ?? '' + property.container?.id ?? '' + property.sortOrder ?? '',
					(property) => {
						// Note: This piece might be moved into the property component
						const inheritedFromMedia = this._ownerMediaTypes?.find(
							(types) => types.containers?.find((containers) => containers.id === property.container?.id),
						);

						return html`<umb-media-type-workspace-view-edit-property
							data-umb-property-id=${ifDefined(property.id)}
							owner-media-type-id=${ifDefined(inheritedFromMedia?.unique)}
							owner-media-type-name=${ifDefined(inheritedFromMedia?.name)}
							?inherited=${property.container?.id !== this.containerId}
							?sort-mode-active=${this._sortModeActive}
							.property=${property}
							@partial-property-update=${(event: CustomEvent) => {
								this._propertyStructureHelper.partialUpdateProperty(property.id, event.detail);
							}}
							@property-delete=${() => {
								this._propertyStructureHelper.removeProperty(property.id!);
							}}>
						</umb-media-type-workspace-view-edit-property>`;
					},
				)}
			</div>
			${!this._sortModeActive
				? html`<uui-button
						label=${this.localize.term('contentTypeEditor_addProperty')}
						id="add"
						look="placeholder"
						href=${ifDefined(this._modalRouteNewProperty)}>
						<umb-localize key="contentTypeEditor_addProperty">Add property</umb-localize>
				  </uui-button> `
				: ''} `;
	}

	static styles = [
		UmbTextStyles,
		css`
			#add {
				width: 100%;
			}
		`,
	];
}

export default UmbMediaTypeWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-type-workspace-view-edit-properties': UmbMediaTypeWorkspaceViewEditPropertiesElement;
	}
}
