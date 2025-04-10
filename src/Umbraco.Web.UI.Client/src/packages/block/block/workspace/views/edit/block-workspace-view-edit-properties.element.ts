import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

import './block-workspace-view-edit-property.element.js';
import type UmbBlockElementManager from '../../block-element-manager.js';

@customElement('umb-block-workspace-view-edit-properties')
export class UmbBlockWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#managerName?: UmbBlockWorkspaceElementManagerNames;
	#workspaceContext?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);
	#properties?: Array<UmbPropertyTypeModel>;
	#visiblePropertiesUniques: Array<string> = [];

	@property({ attribute: false })
	public get managerName(): UmbBlockWorkspaceElementManagerNames | undefined {
		return this.#managerName;
	}
	public set managerName(value: UmbBlockWorkspaceElementManagerNames | undefined) {
		this.#managerName = value;
		this.#setStructureManager();
	}

	@property({ type: String, attribute: 'container-name', reflect: false })
	public get containerId(): string | null | undefined {
		return this.#propertyStructureHelper.getContainerId();
	}
	public set containerId(value: string | null | undefined) {
		this.#propertyStructureHelper.setContainerId(value);
	}

	@state()
	_dataOwner?: UmbBlockElementManager;

	@state()
	_variantId?: UmbVariantId;

	@state()
	_visibleProperties?: Array<UmbPropertyTypeModel>;

	@state()
	private _ownerEntityType?: string;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			this._ownerEntityType = this.#workspaceContext.getEntityType();
			this.observe(
				workspaceContext.variantId,
				(variantId) => {
					this._variantId = variantId;
					this.#processPropertyStructure();
				},
				'observeVariantId',
			);
			this.#setStructureManager();
		});
	}

	#setStructureManager() {
		if (!this.#workspaceContext || !this.#managerName) return;

		this._dataOwner = this.#workspaceContext[this.#managerName];
		const structureManager = this._dataOwner.structure;

		this.#propertyStructureHelper.setStructureManager(structureManager);
		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(properties) => {
				this.#properties = properties;
				this.#processPropertyStructure();
			},
			'observePropertyStructure',
		);
	}

	#processPropertyStructure() {
		if (!this._dataOwner || !this.#properties || !this.#propertyStructureHelper) {
			return;
		}

		const propertyViewGuard = this._dataOwner.propertyViewGuard;

		this.#properties.forEach((property) => {
			const propertyVariantId = new UmbVariantId(this._variantId?.culture, this._variantId?.segment);
			this.observe(propertyViewGuard.isPermittedForVariantAndProperty(propertyVariantId, property), (permitted) => {
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
			});
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
						html`<umb-block-workspace-view-edit-property
							class="property"
							.ownerContext=${this._dataOwner}
							.ownerEntityType=${this._ownerEntityType}
							.variantId=${this._variantId}
							.property=${property}></umb-block-workspace-view-edit-property>`,
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

export default UmbBlockWorkspaceViewEditPropertiesElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-workspace-view-edit-properties': UmbBlockWorkspaceViewEditPropertiesElement;
	}
}
