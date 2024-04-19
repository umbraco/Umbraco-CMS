import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueFilter } from '@umbraco-cms/backoffice/validation';
import { UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

@customElement('umb-content-workspace-view-edit-properties')
export class UmbContentWorkspaceViewEditPropertiesElement extends UmbLitElement {
	@property({ type: String, attribute: 'container-id', reflect: false })
	public get containerId(): string | null | undefined {
		return this.#propertyStructureHelper.getContainerId();
	}
	public set containerId(value: string | null | undefined) {
		this.#propertyStructureHelper.setContainerId(value);
	}

	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

	@state()
	_propertyStructure?: Array<UmbPropertyTypeModel>;

	@state()
	_variantId?: UmbVariantId;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#propertyStructureHelper.setStructureManager(
				// Assuming its the same content model type that we are working with here... [NL]
				workspaceContext.structure as unknown as UmbContentTypeStructureManager<UmbContentTypeModel>,
			);
		});
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			this._variantId = datasetContext.getVariantId();
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
							.dataPath="$.values[${UmbDataPathPropertyValueFilter(property)}].value"
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

export default UmbContentWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-workspace-view-edit-properties': UmbContentWorkspaceViewEditPropertiesElement;
	}
}
