import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../constants.js';
import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_VARIANT_CONTEXT, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';

@customElement('umb-content-workspace-property')
export class UmbContentWorkspacePropertyElement extends UmbLitElement {
	private _alias?: string | undefined;

	@property({ type: String, attribute: 'alias' })
	public get alias(): string | undefined {
		return this._alias;
	}
	public set alias(value: string | undefined) {
		this._alias = value;
		this.#observePropertyType();
	}

	@state()
	private _datasetVariantId?: UmbVariantId;

	@state()
	private _dataPath?: string;

	@state()
	private _viewable?: boolean;

	@state()
	private _writeable?: boolean;

	@state()
	private _workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	private _propertyType?: UmbPropertyTypeModel;

	@state()
	private _hasAccessToSensitiveData = false;

	constructor() {
		super();

		// The Property Dataset is local to the active variant, we use this to retrieve the variant we like to gather the value from.
		this.consumeContext(UMB_VARIANT_CONTEXT, async (variantContext) => {
			this.observe(
				variantContext?.variantId,
				(variantId) => {
					this._datasetVariantId = variantId;
				},
				'observeDatasetVariantId',
			);
		});

		// The Content Workspace Context is used to retrieve the property type we like to observe.
		// This gives us the configuration from the property type as part of the data type.
		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, async (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this.#observePropertyType();
		});

		this.consumeContext(UMB_CURRENT_USER_CONTEXT, (context) => {
			this.observe(context?.hasAccessToSensitiveData, (hasAccessToSensitiveData) => {
				this._hasAccessToSensitiveData = hasAccessToSensitiveData === true;
			});
		});
	}

	async #observePropertyType() {
		if (!this._alias || !this._workspaceContext) return;

		this.observe(
			await this._workspaceContext?.structure.propertyStructureByAlias(this._alias),
			(propertyType) => {
				this._propertyType = propertyType;
			},
			'observePropertyType',
		);
	}

	override willUpdate(changedProperties: Map<string, any>) {
		super.willUpdate(changedProperties);
		if (
			changedProperties.has('_propertyType') ||
			changedProperties.has('_datasetVariantId') ||
			changedProperties.has('_workspaceContext')
		) {
			if (this._datasetVariantId && this._propertyType && this._workspaceContext) {
				const propertyVariantId = new UmbVariantId(
					this._propertyType.variesByCulture ? this._datasetVariantId.culture : null,
					this._propertyType.variesBySegment ? this._datasetVariantId.segment : null,
				);
				this._dataPath = `$.values[${UmbDataPathPropertyValueQuery({
					alias: this._propertyType.alias,
					culture: propertyVariantId.culture,
					segment: propertyVariantId.segment,
				})}].value`;

				this.observe(
					this._workspaceContext.propertyWriteGuard.isPermittedForVariantAndProperty(
						propertyVariantId,
						this._propertyType,
						this._datasetVariantId,
					),
					(write) => {
						this._writeable = write;
					},
					'umbObservePropertyWriteGuard',
				);
				this.observe(
					this._workspaceContext.propertyViewGuard.isPermittedForVariantAndProperty(
						propertyVariantId,
						this._propertyType,
						this._datasetVariantId,
					),
					(permitted) => {
						this._viewable = permitted;
					},
					`umbObservePropertyViewGuard`,
				);
			}
		}
	}

	override render() {
		if (!this._viewable) return nothing;
		if (!this._dataPath || this._writeable === undefined) return nothing;
		if (!this._hasAccessToSensitiveData && this._propertyType?.isSensitive) return nothing;

		return html`<umb-property-type-based-property
			data-path=${this._dataPath}
			.property=${this._propertyType}
			?readonly=${!this._writeable}></umb-property-type-based-property>`;
	}
}

export default UmbContentWorkspacePropertyElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-workspace-property': UmbContentWorkspacePropertyElement;
	}
}
