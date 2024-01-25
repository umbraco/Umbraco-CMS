import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	PropertyContainerTypes,
	// UmbPropertyTypeBasedPropertyElement,
	UmbContentTypeModel} from '@umbraco-cms/backoffice/content-type';
import {
	UmbContentTypePropertyStructureHelper
} from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';

@customElement('umb-block-workspace-view-edit-properties')
export class UmbBlockWorkspaceViewEditPropertiesElement extends UmbLitElement {
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

	_propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

	@state()
	_propertyStructure: Array<PropertyTypeModelBaseModel> = [];

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager(workspaceContext.content.structure);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
		});
	}

	render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property) => html`<umb-property-type-based-property .property=${property}></umb-property-type-based-property> `,
		);
	}

	static styles = [
		UmbTextStyles,
		css`
			umb-property-type-based-property {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			umb-property-type-based-property:last-child {
				border-bottom: 0;
			}
		`,
	];
}

export default UmbBlockWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-properties': UmbBlockWorkspaceViewEditPropertiesElement;
	}
}
