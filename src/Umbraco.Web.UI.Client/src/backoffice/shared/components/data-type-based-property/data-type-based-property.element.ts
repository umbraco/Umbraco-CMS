import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeRepository } from '../../../settings/data-types/repository/data-type.repository';
import type { DataTypeModel, DataTypePropertyModel, DocumentTypePropertyTypeModel } from '@umbraco-cms/backend-api';
import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbObserverController } from '@umbraco-cms/observable-api';

@customElement('umb-datatype-based-property')
export class UmbDataTypeBasedPropertyElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	// TODO: Consider if we just need to get the DataType Key?..
	// TODO: consider if we should make a base type of the DocumentTypePropertyType, which could become the ContentProperty. A shared common type for all properties.
	private _property?: DocumentTypePropertyTypeModel;
	@property({ type: Object, attribute: false })
	public get property(): DocumentTypePropertyTypeModel | undefined {
		return this._property;
	}
	public set property(value: DocumentTypePropertyTypeModel | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataTypeKey !== oldProperty?.dataTypeKey) {
			this._observeDataType(this._property?.dataTypeKey);
		}
	}

	@property()
	value?: object | string;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _dataTypeData: DataTypePropertyModel[] = [];

	private _dataTypeRepository: UmbDataTypeRepository = new UmbDataTypeRepository(this);
	private _dataTypeObserver?: UmbObserverController<DataTypeModel | null>;

	private async _observeDataType(dataTypeKey?: string) {
		this._dataTypeObserver?.destroy();
		if (dataTypeKey) {
			// We do not need to have await here, this is only to ensure that the data is loaded before we try to observe it, and thereby update the DOM with it.
			await this._dataTypeRepository.requestByKey(dataTypeKey);
			this._dataTypeObserver = this.observe(await this._dataTypeRepository.byKey(dataTypeKey), (dataType) => {
				this._dataTypeData = dataType?.data || [];
				this._propertyEditorUiAlias = dataType?.propertyEditorUiAlias || undefined;
			});
		}
	}

	render() {
		return html`<umb-workspace-property
			alias=${ifDefined(this._property?.alias)}
			label=${ifDefined(this._property?.name)}
			description=${ifDefined(this._property?.description || undefined)}
			property-editor-ui-alias=${ifDefined(this._propertyEditorUiAlias)}
			.value=${this.value}
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-datatype-based-property': UmbDataTypeBasedPropertyElement;
	}
}
