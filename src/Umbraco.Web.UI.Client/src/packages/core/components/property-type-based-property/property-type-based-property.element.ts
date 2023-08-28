import { UmbDataTypeConfig } from '../../property-editor/index.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, html, ifDefined, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbDataTypeRepository } from '@umbraco-cms/backoffice/data-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { DataTypeResponseModel, PropertyTypeModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbObserverController } from '@umbraco-cms/backoffice/observable-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
@customElement('umb-property-type-based-property')
export class UmbPropertyTypeBasedPropertyElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public get property(): PropertyTypeModelBaseModel | undefined {
		return this._property;
	}
	public set property(value: PropertyTypeModelBaseModel | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataTypeId !== oldProperty?.dataTypeId) {
			this._observeDataType(this._property?.dataTypeId);
		}
	}
	private _property?: PropertyTypeModelBaseModel;

	@state()
	private _propertyEditorUiAlias?: string;

	@state()
	private _dataTypeData?: UmbDataTypeConfig;

	private _dataTypeRepository: UmbDataTypeRepository = new UmbDataTypeRepository(this);
	private _dataTypeObserver?: UmbObserverController<DataTypeResponseModel | undefined>;



	private async _observeDataType(dataTypeId?: string) {
		this._dataTypeObserver?.destroy();
		if (dataTypeId) {
			// Its not technically needed to have await here, this is only to ensure that the data is loaded before we observe it, and thereby only updating the DOM with the latest data.
			await this._dataTypeRepository.requestById(dataTypeId);
			this._dataTypeObserver = this.observe(
				await this._dataTypeRepository.byId(dataTypeId),
				(dataType) => {
					this._dataTypeData = dataType?.values;
					this._propertyEditorUiAlias = dataType?.propertyEditorUiAlias || undefined;
					// If there is no UI, we will look up the Property editor model to find the default UI alias:
					if (!this._propertyEditorUiAlias && dataType?.propertyEditorAlias) {
						//use 'dataType.propertyEditorAlias' to look up the extension in the registry:
						this.observe(
							umbExtensionsRegistry.getByTypeAndAlias('propertyEditorSchema', dataType.propertyEditorAlias),
							(extension) => {
								if (!extension) return;
								this._propertyEditorUiAlias = extension?.meta.defaultPropertyEditorUiAlias;
								this.removeControllerByAlias('_observePropertyEditorSchema');
							},
							'_observePropertyEditorSchema'
						);
					}
				},
				'_observeDataType'
			);
		}
	}

	render() {
		return html`<umb-workspace-property
			alias=${ifDefined(this._property?.alias)}
			label=${ifDefined(this._property?.name)}
			description=${ifDefined(this._property?.description || undefined)}
			property-editor-ui-alias=${ifDefined(this._propertyEditorUiAlias)}
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}

	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-type-based-property': UmbPropertyTypeBasedPropertyElement;
	}
}
