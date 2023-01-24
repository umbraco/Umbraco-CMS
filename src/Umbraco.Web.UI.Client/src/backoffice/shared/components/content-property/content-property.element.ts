import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html } from 'lit';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';

import {  UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN } from '../../../settings/data-types/data-type.detail.store';
import type { UmbDataTypeDetailStore } from '../../../settings/data-types/data-type.detail.store';
import type { ContentProperty, DataTypeDetails, DataTypePropertyData } from '@umbraco-cms/models';

import '../workspace-property/workspace-property.element';
import { UmbLitElement } from '@umbraco-cms/element';
import { UmbObserverController } from '@umbraco-cms/observable-api';

@customElement('umb-content-property')
export class UmbContentPropertyElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	// TODO: Consider if we just need to get the DataType Key?..
	private _property?: ContentProperty;
	@property({ type: Object, attribute: false })
	public get property(): ContentProperty | undefined {
		return this._property;
	}
	public set property(value: ContentProperty | undefined) {
		const oldProperty = this._property;
		this._property = value;
		if (this._property?.dataTypeKey !== oldProperty?.dataTypeKey) {
			this._observeDataType(this._property?.dataTypeKey);
		}
	}

	@property()
	value?: object | string;

	@state()
	private _propertyEditorUIAlias?: string;

	@state()
	private _dataTypeData: DataTypePropertyData[] = [];

	private _dataTypeStore?: UmbDataTypeDetailStore;
	private _dataTypeObserver?: UmbObserverController<DataTypeDetails | null>;

	constructor() {
		super();

		this.consumeContext(UMB_DATA_TYPE_DETAIL_STORE_CONTEXT_TOKEN, (instance) => {
			this._dataTypeStore = instance;
			this._observeDataType(this._property?.dataTypeKey);
		});
	}

	private _observeDataType(dataTypeKey?: string) {
		if (!this._dataTypeStore) return;

		this._dataTypeObserver?.destroy();
		if (dataTypeKey) {
			this._dataTypeObserver = this.observe(this._dataTypeStore.getByKey(dataTypeKey), (dataType) => {
				this._dataTypeData = dataType?.data || [];
				this._propertyEditorUIAlias = dataType?.propertyEditorUIAlias || undefined;
			});
		}
	}

	render() {
		return html`<umb-workspace-property
			alias=${ifDefined(this._property?.alias)}
			label=${ifDefined(this._property?.label)}
			description=${ifDefined(this._property?.description)}
			property-editor-ui-alias="${ifDefined(this._propertyEditorUIAlias)}"
			.value=${this.value}
			.config=${this._dataTypeData}></umb-workspace-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-property': UmbContentPropertyElement;
	}
}
