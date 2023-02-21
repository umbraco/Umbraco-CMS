import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeRepository } from '../../../settings/data-types/repository/data-type.repository';
import { UmbVariantId } from '../../variants/variant-id.class';
import { UmbDocumentWorkspaceContext } from '../../../documents/documents/workspace/document-workspace.context';
import type { DataTypeModel, DataTypePropertyModel, PropertyTypeViewModelBaseModel } from '@umbraco-cms/backend-api';
import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbObserverController } from '@umbraco-cms/observable-api';

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

	private _property?: PropertyTypeViewModelBaseModel;
	@property({ type: Object, attribute: false })
	public get property(): PropertyTypeViewModelBaseModel | undefined {
		return this._property;
	}
	public set property(value: PropertyTypeViewModelBaseModel | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataTypeKey !== oldProperty?.dataTypeKey) {
			this._observeDataType(this._property?.dataTypeKey);
			this._observeProperty();
		}
	}

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _dataTypeData: DataTypePropertyModel[] = [];

	private _dataTypeRepository: UmbDataTypeRepository = new UmbDataTypeRepository(this);
	private _dataTypeObserver?: UmbObserverController<DataTypeModel | null>;

	@state()
	private _value?: unknown;

	/**
	 * VariantId. A Variant Configuration to identify which the variant of its value.
	 * @public
	 * @type {UmbVariantId}
	 * @attr
	 * @default undefined
	 */
	@property({ type: Object, attribute: false })
	private _variantId?: UmbVariantId;

	private _workspaceContext?: UmbDocumentWorkspaceContext;

	constructor() {
		super();
		this.consumeContext('umbWorkspaceContext', (workspaceContext: UmbDocumentWorkspaceContext) => {
			this._workspaceContext = workspaceContext;
			this._observeProperty();
		});
	}

	private _observeProperty() {
		if (!this._workspaceContext || !this.property || !this._property?.alias) return;

		this.observe(
			this._workspaceContext.propertyValueByAlias(this._property.alias, this._variantId),
			(value) => {
				this._value = value;
			},
			'_observePropertyValueByAlias'
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
					this._dataTypeData = dataType?.data || [];
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
			.variantId=${this._variantId}
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-based-property': UmbPropertyTypeBasedPropertyElement;
	}
}
