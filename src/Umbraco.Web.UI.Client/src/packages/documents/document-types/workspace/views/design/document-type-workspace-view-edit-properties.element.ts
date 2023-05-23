import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { ifDefined } from 'lit/directives/if-defined.js';
import { UmbDocumentTypeWorkspaceContext } from '../../document-type-workspace.context';
import { UmbContentTypePropertyStructureHelper, PropertyContainerTypes } from '@umbraco-cms/backoffice/content-type';
import { UmbSorterController, UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	DocumentTypePropertyTypeResponseModel,
	PropertyTypeResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UMB_MODAL_CONTEXT_TOKEN, UMB_PROPERTY_SETTINGS_MODAL } from '@umbraco-cms/backoffice/modal';
import './document-type-workspace-view-edit-property.element';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
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
	_propertyStructure: Array<PropertyTypeResponseModelBaseModel> = [];

	#modalContext?: typeof UMB_MODAL_CONTEXT_TOKEN.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager(
				(workspaceContext as UmbDocumentTypeWorkspaceContext).structure
			);
		});
		this.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance) => (this.#modalContext = instance));
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
			this.#propertySorter.setModel(this._propertyStructure);
		});
	}

	async #onAddProperty() {
		const property = await this._propertyStructureHelper.addProperty(this._containerId);
		if (!property) return;

		// TODO: Figure out how we from this location can get into the routeable modal..
		/*
		// Take id and parse to modal:
		console.log('property id:', property.id!, property);

		// TODO: route modal..
		const modalHandler = this.#modalContext?.open(UMB_PROPERTY_SETTINGS_MODAL);

		modalHandler?.onSubmit().then((result) => {
			console.log(result);
		});
		*/
	}

	render() {
		return html`<div id="property-list">
				${repeat(
					this._propertyStructure,
					(property) => property.alias ?? '' + property.containerId ?? '' + (property as any).sortOrder ?? '',
					(property) =>
						html`<document-type-workspace-view-edit-property
							class="property"
							data-umb-property-id=${property.id}
							data-property-container-is=${property.containerId}
							data-container-id=${this.containerId}
							?inherited=${ifDefined(property.containerId !== this.containerId)}
							.property=${property}
							@partial-property-update=${(event: CustomEvent) => {
								this._propertyStructureHelper.partialUpdateProperty(property.id, event.detail);
							}}></document-type-workspace-view-edit-property>`
				)}
			</div>
			<uui-button id="add" look="placeholder" @click=${this.#onAddProperty}> Add property </uui-button> `;
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
