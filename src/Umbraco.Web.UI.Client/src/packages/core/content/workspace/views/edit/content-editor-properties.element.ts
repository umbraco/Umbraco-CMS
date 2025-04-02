import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/workspace';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
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
	_variantId?: UmbVariantId;

	@state()
	_propertyStructure?: Array<UmbPropertyTypeModel>;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			this._variantId = datasetContext.getVariantId();
		});

		this.consumeContext(UMB_PROPERTY_STRUCTURE_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#propertyStructureHelper.setStructureManager(
				// Assuming its the same content model type that we are working with here... [NL]
				workspaceContext.structure as unknown as UmbContentTypeStructureManager<UmbContentTypeModel>,
			);
		});

		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
			},
			null,
		);
	}

	/*
	#isViewablePropertyType(property: UmbPropertyTypeModel) {
		// The state is not running, so the property is viewable by default.
		if (this._propertyViewStateIsRunning === false) {
			return true;
		}

		const propertyVariantId = this.#getPropertyVariantId(property);
		return this._propertyViewStates.some(
			(state) => state.propertyType.unique === property.unique && state.propertyType.variantId.equal(propertyVariantId),
		);
	}

	#isWritablePropertyType(property: UmbPropertyTypeModel) {
		// The state is not running, so the property is writable by default.
		if (this._propertyWriteStateIsRunning === false) {
			return true;
		}

		const propertyVariantId = this.#getPropertyVariantId(property);

		// Check if the property is writable
		const isWriteAllowed = this._propertyWriteStates.some(
			(state) => state.propertyType.unique === property.unique && state.propertyType.variantId.equal(propertyVariantId),
		);

		// Check if the property is read only
		const isReadOnly = this._propertyReadOnlyStates.some(
			(state) => state.propertyType.unique === property.unique && state.propertyType.variantId.equal(propertyVariantId),
		);

		// If the property has a read only state it will override the write state
		// and the property will always be read only
		return isWriteAllowed && !isReadOnly;
	}

	#getPropertyVariantId(property: UmbPropertyTypeModel) {
		return new UmbVariantId(
			property.variesByCulture ? this.#variantId!.culture : null,
			property.variesBySegment ? this.#variantId!.segment : null,
		);
	}
		*/

	override render() {
		return this._variantId && this._propertyStructure
			? repeat(
					this._propertyStructure,
					(property) => property.alias,
					(property) =>
						html`<umb-content-workspace-view-edit-property
							class="property"
							.variantId=${this._variantId}
							.property=${property}></umb-content-workspace-view-edit-property>`,
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
