import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement, umbDestroyOnDisconnect } from '@umbraco-cms/backoffice/lit-element';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';

@customElement('umb-block-workspace-view-edit-properties')
export class UmbBlockWorkspaceViewEditPropertiesElement extends UmbLitElement {
	#managerName?: UmbBlockWorkspaceElementManagerNames;
	#blockWorkspace?: typeof UMB_BLOCK_WORKSPACE_CONTEXT.TYPE;
	#propertyStructureHelper = new UmbContentTypePropertyStructureHelper<UmbContentTypeModel>(this);

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
	_propertyStructure: Array<UmbPropertyTypeModel> = [];

	@state()
	_dataPaths?: Array<string>;

	@state()
	private _ownerEntityType?: string;

	#variantId?: UmbVariantId;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#blockWorkspace = workspaceContext;
			this._ownerEntityType = this.#blockWorkspace.getEntityType();
			this.observe(
				workspaceContext.variantId,
				(variantId) => {
					this.#variantId = variantId;
					this.#generatePropertyDataPath();
				},
				'observeVariantId',
			);
			this.#setStructureManager();
		});
	}

	#setStructureManager() {
		if (!this.#blockWorkspace || !this.#managerName) return;
		this.#propertyStructureHelper.setStructureManager(this.#blockWorkspace[this.#managerName].structure);
		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
				this.#generatePropertyDataPath();
			},
			'observePropertyStructure',
		);
	}

	/*
	#generatePropertyDataPath() {
		if (!this._propertyStructure) return;
		this._dataPaths = this._propertyStructure.map((property) => `$.${property.alias}`);
	}
		*/

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

	override render() {
		return repeat(
			this._propertyStructure,
			(property) => property.alias,
			(property, index) =>
				html`<umb-property-type-based-property
					class="property"
					data-path=${this._dataPaths![index]}
					.ownerEntityType=${this._ownerEntityType}
					.property=${property}
					${umbDestroyOnDisconnect()}></umb-property-type-based-property>`,
		);
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
