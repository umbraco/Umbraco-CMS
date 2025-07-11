import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../constants.js';
import { html, customElement, property, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbDataPathPropertyValueQuery } from '@umbraco-cms/backoffice/validation';

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
	_datasetVariantId?: UmbVariantId;

	@state()
	_dataPath?: string;

	@state()
	_viewable?: boolean;

	@state()
	_writeable?: boolean;

	@state()
	_workspaceContext?: typeof UMB_CONTENT_WORKSPACE_CONTEXT.TYPE;

	@state()
	_propertyType?: UmbPropertyTypeModel;

	constructor() {
		super();

		// The Property Dataset is local to the active variant, we use this to retrieve the variant we like to gather the value from.
		this.consumeContext(UMB_PROPERTY_DATASET_CONTEXT, (datasetContext) => {
			this._datasetVariantId = datasetContext?.getVariantId();
		});

		// The Content Workspace Context is used to retrieve the property type we like to observe.
		// This gives us the configuration from the property type as part of the data type.
		this.consumeContext(UMB_CONTENT_WORKSPACE_CONTEXT, async (workspaceContext) => {
			this._workspaceContext = workspaceContext;
			this.#observePropertyType();
		});
	}

	async #observePropertyType() {
		if (!this._alias || !this._workspaceContext) return;

		this.observe(await this._workspaceContext?.structure.propertyStructureByAlias(this._alias), (propertyType) => {
			this._propertyType = propertyType;
			this.#checkViewGuard();
		});
	}

	#checkViewGuard() {
		if (!this._workspaceContext || !this._propertyType || !this._datasetVariantId) return;

		const propertyVariantId = new UmbVariantId(
			this._propertyType.variesByCulture ? this._datasetVariantId.culture : null,
			this._propertyType.variesBySegment ? this._datasetVariantId.segment : null,
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
						propertyVariantId,
					),
					(write) => {
						this._writeable = write;
					},
					'observeView',
				);
			}
		}
	}

	override render() {
		if (!this._viewable) return nothing;
		if (!this._dataPath || this._writeable === undefined) return nothing;

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
