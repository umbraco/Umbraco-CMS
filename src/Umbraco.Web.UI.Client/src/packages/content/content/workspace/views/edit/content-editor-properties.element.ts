import type { UMB_CONTENT_WORKSPACE_CONTEXT } from '../../content-workspace.context-token.js';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type {
	UmbContentTypeModel,
	UmbContentTypeStructureManager,
	UmbPropertyTypeModel,
} from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

import './content-editor-property.element.js';

@customElement('umb-content-workspace-view-edit-properties')
export class UmbContentWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;
	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);
	#properties?: Array<UmbPropertyTypeModel>;
	#visiblePropertiesUniques: Array<string> = [];

	@property({ type: String, attribute: 'container-id', reflect: false })
	public get containerId(): string | null | undefined {
		return this.#propertyStructureHelper.getContainerId();
	}
	public set containerId(value: string | null | undefined) {
		this.#propertyStructureHelper.setContainerId(value);
	}

	@state()
	_variantId?: UmbVariantId;

	@state()
	_visibleProperties?: Array<UmbPropertyTypeModel>;

	constructor() {
		super();

		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			this._variantId = datasetContext.getVariantId();
			this.#processPropertyStructure();
		});

		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;

			this.#propertyStructureHelper.setStructureManager(
				// Assuming its the same content model type that we are working with here... [NL]
				workspaceContext.structure as unknown as UmbContentTypeStructureManager<UmbContentTypeModel>,
			);

			this.observe(
				this.#propertyStructureHelper.propertyStructure,
				(properties) => {
					this.#properties = properties;
					this.#processPropertyStructure();
				},
				'observePropertyStructure',
			);
		});
	}

	#processPropertyStructure() {
		if (!this.#workspaceContext || !this.#properties || !this.#propertyStructureHelper) {
			return;
		}

		const propertyViewGuard = this.#workspaceContext.propertyViewGuard;

		this.#properties.forEach((property) => {
			const propertyVariantId = new UmbVariantId(this._variantId?.culture, this._variantId?.segment);
			this.observe(
				propertyViewGuard.isPermittedForVariantAndProperty(propertyVariantId, property),
				(permitted) => {
					if (permitted) {
						this.#visiblePropertiesUniques.push(property.unique);
						this.#calculateVisibleProperties();
					} else {
						const index = this.#visiblePropertiesUniques.indexOf(property.unique);
						if (index !== -1) {
							this.#visiblePropertiesUniques.splice(index, 1);
							this.#calculateVisibleProperties();
						}
					}
				},
				`propertyViewGuard-permittedForVariantAndProperty-${property.unique}`,
			);
		});
	}

	#calculateVisibleProperties() {
		this._visibleProperties = this.#properties!.filter((property) =>
			this.#visiblePropertiesUniques.includes(property.unique),
		);
	}

	override render() {
		return this._variantId && this._visibleProperties
			? repeat(
					this._visibleProperties,
					(property) => property.alias,
					(property) =>
						html`<umb-content-workspace-view-edit-property
							class="property"
							.variantId=${this._variantId}
							.property=${property}></umb-content-workspace-view-edit-property>`,
				)
			: nothing;
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
