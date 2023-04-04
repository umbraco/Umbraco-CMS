import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { ifDefined } from 'lit/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeRepository } from '../../../settings/data-types/repository/data-type.repository';
import { UmbVariantId } from '../../variants/variant-id.class';
import { UmbDocumentWorkspaceContext } from '../../../documents/documents/workspace/document-workspace.context';
import type {
	DataTypeResponseModel,
	DataTypePropertyPresentationModel,
	PropertyTypeResponseModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { UMB_ENTITY_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/context-api';

@customElement('umb-property-type-based-property')
export class UmbPropertyTypeBasedPropertyElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	@property({ type: Object, attribute: false })
	public get property(): PropertyTypeResponseModelBaseModel | undefined {
		return this._property;
	}
	public set property(value: PropertyTypeResponseModelBaseModel | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataTypeKey !== oldProperty?.dataTypeKey) {
			this._observeDataType(this._property?.dataTypeKey);
			this._observeProperty();
		}
	}
	private _property?: PropertyTypeResponseModelBaseModel;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _dataTypeData: DataTypePropertyPresentationModel[] = [];

	private _dataTypeRepository: UmbDataTypeRepository = new UmbDataTypeRepository(this);
	private _dataTypeObserver?: UmbObserverController<DataTypeResponseModel | undefined>;

	@state()
	private _value?: unknown;

	/**
	 * propertyVariantId. A VariantID to identify which the variant of this properties value.
	 * @public
	 * @type {UmbVariantId}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Object, attribute: false })
	public get propertyVariantId(): UmbVariantId | undefined {
		return this._propertyVariantId;
	}
	public set propertyVariantId(value: UmbVariantId | undefined) {
		const oldValue = this._propertyVariantId;
		if (value && oldValue?.equal(value)) return;
		this._propertyVariantId = value;
		this._observeProperty();
		this.requestUpdate('propertyVariantId', oldValue);
	}
	private _propertyVariantId?: UmbVariantId | undefined;

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();
		this.consumeContext(UMB_ENTITY_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._workspaceContext = workspaceContext as UmbDocumentWorkspaceContext;
			this._observeProperty();
		});
	}

	private _observePropertyValue?: UmbObserverController<unknown>;
	private _observeProperty() {
		if (!this._workspaceContext || !this.property || !this._property?.alias) return;

		this._observePropertyValue?.destroy();
		this._observePropertyValue = this.observe(
			this._workspaceContext.propertyValueByAlias(this._property.alias, this._propertyVariantId),
			(value) => {
				this._value = value;
			}
		);
	}

	private async _observeDataType(dataTypeKey?: string) {
		this._dataTypeObserver?.destroy();
		if (dataTypeKey) {
			// Its not technically needed to have await here, this is only to ensure that the data is loaded before we observe it, and thereby only updating the DOM with the latest data.
			await this._dataTypeRepository.requestByKey(dataTypeKey);
			this._dataTypeObserver = this.observe(
				await this._dataTypeRepository.byKey(dataTypeKey),
				(dataType) => {
					this._dataTypeData = dataType?.values || [];
					this._propertyEditorUiAlias = dataType?.propertyEditorUiAlias || undefined;
				},
				'observeDataType'
			);
		}
	}

	render() {
		return html`<umb-workspace-property
			alias=${ifDefined(this._property?.alias)}
			label=${ifDefined(this._property?.name)}
			description=${ifDefined(this._property?.description || undefined)}
			property-editor-ui-alias=${ifDefined(this._propertyEditorUiAlias)}
			.value=${this._value}
			.propertyVariantId=${this.propertyVariantId}
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-based-property': UmbPropertyTypeBasedPropertyElement;
	}
}
