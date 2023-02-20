import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeRepository } from '../../../settings/data-types/repository/data-type.repository';
import { UmbWorkspacePropertySetContext } from '../workspace/workspace-context/workspace-property-set.context';
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

	private _propertySetContext?: UmbWorkspacePropertySetContext;

	constructor() {
		super();
		this.consumeContext('umbWorkspacePropertySetContext', (propertySetContext: UmbWorkspacePropertySetContext) => {
			this._propertySetContext = propertySetContext;
			this._observeProperty();
		});
	}

	private _observeProperty() {
		if (!this._propertySetContext || !this.property || !this._property?.alias) return;

		console.log('_observeProperty');

		this.observe(
			this._propertySetContext.propertyValueByAlias(this._property.alias),
			(value) => {
				console.log('got value', value);
				this._value = value;
			},
			'observeValue'
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
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-based-property': UmbPropertyTypeBasedPropertyElement;
	}
}
