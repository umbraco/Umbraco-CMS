import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { repeat } from 'lit/directives/repeat.js';
import { UmbDocumentWorkspaceContext } from '../../document-workspace.context';
import { UmbContentTypePropertyStructureHelper, PropertyContainerTypes } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { PropertyTypeResponseModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
@customElement('umb-document-workspace-view-edit-properties')
export class UmbDocumentWorkspaceViewEditPropertiesElement extends UmbLitElement {
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

	constructor() {
		super();

		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._propertyStructureHelper.setStructureManager((workspaceContext as UmbDocumentWorkspaceContext).structure);
		});
		this.observe(this._propertyStructureHelper.propertyStructure, (propertyStructure) => {
			this._propertyStructure = propertyStructure;
		});
	}

	render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property) => html`<umb-variantable-property class="property" .property=${property}></umb-variantable-property> `
		);
	}

	static styles = [
		UUITextStyles,
		css`
			.property {
				border-bottom: 1px solid var(--uui-color-divider);
			}
			.property:last-child {
				border-bottom: 0;
			}
		`,
	];
}

export default UmbDocumentWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-edit-properties': UmbDocumentWorkspaceViewEditPropertiesElement;
	}
}
