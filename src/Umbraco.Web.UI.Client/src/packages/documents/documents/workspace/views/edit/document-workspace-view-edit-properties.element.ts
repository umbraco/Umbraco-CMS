import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '../../document-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbPropertyContainerTypes, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';

@customElement('umb-document-workspace-view-edit-properties')
export class UmbDocumentWorkspaceViewEditPropertiesElement extends UmbLitElement {
	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerName(): string | undefined {
		return this.#propertyStructureHelper.getContainerName();
	}
	public set containerName(value: string | undefined) {
		this.#propertyStructureHelper.setContainerName(value);
	}

	@property({ type: String, attribute: 'container-type', reflect: false })
	public get containerType(): UmbPropertyContainerTypes | undefined {
		return this.#propertyStructureHelper.getContainerType();
	}
	public set containerType(value: UmbPropertyContainerTypes | undefined) {
		this.#propertyStructureHelper.setContainerType(value);
	}

	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbDocumentTypeDetailModel>(this);

	@state()
	_propertyStructure?: Array<UmbPropertyTypeModel>;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#propertyStructureHelper.setStructureManager(workspaceContext.structure);
		});
		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
			},
			null,
		);
	}

	render() {
		return this._propertyStructure
			? repeat(
					this._propertyStructure,
					(property) => property.alias,
					(property) =>
						html`<umb-property-type-based-property
							class="property"
							.property=${property}></umb-property-type-based-property> `,
				)
			: '';
	}

	static styles = [
		UmbTextStyles,
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
