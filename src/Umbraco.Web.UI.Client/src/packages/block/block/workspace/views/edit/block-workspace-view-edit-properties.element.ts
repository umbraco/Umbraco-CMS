import type { UmbBlockWorkspaceElementManagerNames } from '../../block-workspace.context.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from '../../block-workspace.context-token.js';
import { css, html, customElement, property, state, repeat } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypePropertyStructureHelper } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement, umbDestroyOnDisconnect } from '@umbraco-cms/backoffice/lit-element';
import type { UmbVariantPropertyViewState, UmbVariantPropertyWriteState } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

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

	@state()
	_propertyViewStateIsRunning = true;

	@state()
	_propertyViewStates: Array<UmbVariantPropertyViewState> = [];

	@state()
	_propertyWriteStateIsRunning = true;

	@state()
	_propertyWriteStates: Array<UmbVariantPropertyWriteState> = [];

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

		const structureManager = this.#blockWorkspace[this.#managerName].structure;

		this.#propertyStructureHelper.setStructureManager(structureManager);
		this.observe(
			this.#propertyStructureHelper.propertyStructure,
			(propertyStructure) => {
				this._propertyStructure = propertyStructure;
				this.#generatePropertyDataPath();
			},
			'observePropertyStructure',
		);

		this.observe(
			observeMultiple([structureManager.propertyViewState.isRunning, structureManager.propertyViewState.states]),
			([isRunning, states]) => {
				this._propertyViewStateIsRunning = isRunning;
				this._propertyViewStates = states;
			},
			'umbObservePropertyViewStates',
		);

		this.observe(
			observeMultiple([structureManager.propertyWriteState.isRunning, structureManager.propertyWriteState.states]),
			([isEnabled, states]) => {
				this._propertyWriteStateIsRunning = isEnabled;
				this._propertyWriteStates = states;
			},
			'umbObservePropertyWriteStates',
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

	#getVisibleProperties() {
		return this._propertyStructure?.filter((property) => this.#isViewablePropertyType(property)) || [];
	}

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
		return this._propertyWriteStates.some(
			(state) => state.propertyType.unique === property.unique && state.propertyType.variantId.equal(propertyVariantId),
		);
	}

	#getPropertyVariantId(property: UmbPropertyTypeModel) {
		return new UmbVariantId(
			property.variesByCulture ? this.#variantId!.culture : null,
			property.variesBySegment ? this.#variantId!.segment : null,
		);
	}

	override render() {
		return repeat(
			this.#getVisibleProperties(),
			(property) => property.alias,
			(property, index) =>
				html`<umb-property-type-based-property
					class="property"
					data-path=${this._dataPaths![index]}
					.ownerEntityType=${this._ownerEntityType}
					.property=${property}
					?readonly=${!this.#isWritablePropertyType(property)}
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
