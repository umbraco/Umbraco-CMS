import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context.js';
import './document-type-workspace-view-edit-property.element.js';
import { css, html, customElement, property, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbContentTypePropertyStructureHelper, PropertyContainerTypes } from '@umbraco-cms/backoffice/content-type';
import { UmbSorterController, UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	DocumentTypePropertyTypeResponseModel,
	DocumentTypeResponseModel,
	PropertyTypeModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UMB_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UMB_PROPERTY_SETTINGS_MODAL, UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/modal';
const SORTER_CONFIG: UmbSorterConfig<DocumentTypePropertyTypeResponseModel> = {
	compareElementToModel: (element: HTMLElement, model: DocumentTypePropertyTypeResponseModel) => {
		return element.getAttribute('data-umb-property-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: DocumentTypePropertyTypeResponseModel) => {
		return container.querySelector('data-umb-property-id[' + modelEntry.id + ']');
	},
	identifier: 'content-type-property-sorter',
	itemSelector: '[data-umb-property-id]',
	disabledItemSelector: '[inherited]',
	containerSelector: '#property-list',
};
@customElement('umb-document-type-workspace-view-edit-properties')
export class UmbDocumentTypeWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#propertySorter = new UmbSorterController(this, {
		...SORTER_CONFIG,
		performItemInsert: (args) => {
			let sortOrder = 0;
			if (this._propertyStructure.length > 0) {
				if (args.newIndex === 0) {
					// TODO: Remove 'as any' when sortOrder is added to the model:
					sortOrder = ((this._propertyStructure[0] as any).sortOrder ?? 0) - 1;
				} else {
					sortOrder =
						((this._propertyStructure[Math.min(args.newIndex, this._propertyStructure.length - 1)] as any).sortOrder ??
							0) + 1;
				}
			}
			return this._propertyStructureHelper.insertProperty(args.item, sortOrder);
		},
		performItemRemove: (args) => {
			return this._propertyStructureHelper.removeProperty(args.item.id!);
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
	public get containerType(): PropertyContainerTypes | undefined {
		return this._propertyStructureHelper.getContainerType();
	}
	public set containerType(value: PropertyContainerTypes | undefined) {
		this._propertyStructureHelper.setContainerType(value);
	}

	_propertyStructureHelper = new UmbContentTypePropertyStructureHelper(this);

	@state()
	_propertyStructure: Array<PropertyTypeModelBaseModel> = [];

	@state()
	_ownerDocumentTypes?: DocumentTypeResponseModel[];

	@state()
	protected _modalRouteNewProperty?: string;

	constructor() {
		super();

		this.consumeContext(UMB_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager(
				(workspaceContext as UmbDocumentTypeWorkspaceContext).structure,
			);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
			this.#propertySorter.setModel(this._propertyStructure);
		});

		// Note: Route for adding a new property
		new UmbModalRouteRegistrationController(this, UMB_PROPERTY_SETTINGS_MODAL)
			.addAdditionalPath('new-property')
			.onSetup(async () => {
				return (await this._propertyStructureHelper.createPropertyScaffold(this._containerId)) ?? false;
			})
			.onSubmit((result) => {
				this.#addProperty(result);
			})
			.observeRouteBuilder((routeBuilder) => {
				this._modalRouteNewProperty = routeBuilder(null);
			});
	}

	connectedCallback(): void {
		super.connectedCallback();
		const doctypes = this._propertyStructureHelper.getOwnerDocumentTypes();
		if (!doctypes) return;
		this.observe(
			doctypes,
			(documents) => {
				this._ownerDocumentTypes = documents;
			},
			'observeOwnerDocumentTypes',
		);
	}

	async #addProperty(propertyData: PropertyTypeModelBaseModel) {
		const propertyPlaceholder = await this._propertyStructureHelper.addProperty(this._containerId);
		if (!propertyPlaceholder) return;

		this._propertyStructureHelper.partialUpdateProperty(propertyPlaceholder.id, propertyData);
	}

	render() {
		return html`<div id="property-list">
				${repeat(
					this._propertyStructure,
					(property) => property.id ?? '' + property.containerId ?? '' + (property as any).sortOrder ?? '',
					(property) => {
						// Note: This piece might be moved into the property component
						const inheritedFromDocument = this._ownerDocumentTypes?.find(
							(types) => types.containers?.find((containers) => containers.id === property.containerId),
						);

						return html`<document-type-workspace-view-edit-property
							class="property"
							data-umb-property-id=${ifDefined(property.id)}
							owner-document-type-id=${ifDefined(inheritedFromDocument?.id)}
							owner-document-type-name=${ifDefined(inheritedFromDocument?.name)}
							?inherited=${property.containerId !== this.containerId}
							.property=${property}
							@partial-property-update=${(event: CustomEvent) => {
								this._propertyStructureHelper.partialUpdateProperty(property.id, event.detail);
							}}
							@property-delete=${() => {
								this._propertyStructureHelper.removeProperty(property.id!);
							}}>
						</document-type-workspace-view-edit-property>`;
					},
				)}
			</div>
			<uui-button
				label=${this.localize.term('contentTypeEditor_addProprety')}
				id="add"
				look="placeholder"
				href=${ifDefined(this._modalRouteNewProperty)}>
				<umb-localize key="contentTypeEditor_addProprety">Add property</umb-localize>
			</uui-button> `;
	}

	static styles = [
		UUITextStyles,
		css`
			#add {
				width: 100%;
			}
		`,
	];
}

export default UmbDocumentTypeWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-type-workspace-view-edit-properties': UmbDocumentTypeWorkspaceViewEditPropertiesElement;
	}
}
