import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
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
	#variantId?: UmbVariantId;

	@state()
	_propertyStructure?: Array<UmbPropertyTypeModel>;

	@state()
	_dataPaths?: Array<string>;

	@state()
	_readablePropertyUniques: Array<string> = [];

	@state()
	_writeablePropertyUniques: Array<string> = [];

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#propertyStructureHelper.setStructureManager(
				// Assuming its the same content model type that we are working with here... [NL]
				workspaceContext.structure as unknown as UmbContentTypeStructureManager<UmbContentTypeModel>,
			);

			this.observe(workspaceContext.structure.propertyReadState.states, (states) => {
				this._readablePropertyUniques = states.map((state) => state.propertyType.unique) ?? [];
			});

			this.observe(workspaceContext.structure.propertyWriteState.states, (states) => {
				this._writeablePropertyUniques = states.map((state) => state.propertyType.unique) ?? [];
			});
		});

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			debugger;
			this.#variantId = datasetContext.getVariantId();
			this.#generatePropertyDataPath();
		});

		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
				this.#generatePropertyDataPath();
			},
			null,
		);
	}

	#generatePropertyDataPath() {
		if (!this.#variantId || !this._propertyStructure) return;
		this._dataPaths = this._propertyStructure.map(
			(property) =>
				`$.values[${UmbDataPathPropertyValueQuery({
					alias: property.alias,
					culture: property.variesByCulture ? this.#variantId!.culture : null,
					segment: property.variesBySegment ? this.#variantId!.segment : null,
				})}].value`,
		);
	}

	#getVisibleProperties() {
		return this._propertyStructure?.filter((property) => this._readablePropertyUniques.includes(property.unique)) ?? [];
	}

	#isWritable(unique: string) {
		return this._writeablePropertyUniques.includes(unique);
	}

	override render() {
		return this._propertyStructure && this._dataPaths
			? repeat(
					this.#getVisibleProperties(),
					(property) => property.alias,
					(property, index) =>
						html`<umb-property-type-based-property
							class="property"
							data-path=${this._dataPaths![index]}
							.property=${property}
							?readonly=${this.#isWritable(property.unique)}></umb-property-type-based-property> `,
				)
			: '';
	}

	static override styles = [
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
