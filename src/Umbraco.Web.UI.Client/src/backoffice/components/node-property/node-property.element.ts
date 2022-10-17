import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { ifDefined } from 'lit-html/directives/if-defined.js';
import { customElement, property, state } from 'lit/decorators.js';
import { EMPTY, of, switchMap } from 'rxjs';

import { UmbDataTypeStore } from '../../../core/stores/data-type/data-type.store';
import { NodeProperty } from '../../../core/mocks/data/node.data';
import { UmbObserverMixin } from '../../../core/observable-api';
import type { ManifestTypes } from '../../../core/models';
import { UmbExtensionRegistry } from '@umbraco-cms/extensions-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

import '../entity-property/entity-property.element';

@customElement('umb-node-property')
export class UmbNodePropertyElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
			}
		`,
	];

	private _property?: NodeProperty;
	@property({ type: Object, attribute: false })
	public get property(): NodeProperty | undefined {
		return this._property;
	}
	public set property(value: NodeProperty | undefined) {
		this._property = value;
		this._observeDataType();
	}

	@property()
	value?: string;

	@state()
	private _propertyEditorUIAlias?: string;

	@state()
	private _dataTypeData?: any;

	private _extensionRegistry?: UmbExtensionRegistry;
	private _dataTypeStore?: UmbDataTypeStore;

	constructor() {
		super();

		this.consumeAllContexts(['umbDataTypeStore', 'umbExtensionRegistry'], (instances) => {
			this._extensionRegistry = instances['umbExtensionRegistry'];
			this._dataTypeStore = instances['umbDataTypeStore'];
			this._observeDataType();
		});
	}

	private _observeDataType() {
		if (!this._dataTypeStore || !this._extensionRegistry || !this._property) return;

		this.observe<ManifestTypes>(
			this._dataTypeStore.getByKey(this._property.dataTypeKey).pipe(
				switchMap((dataType) => {
					if (!dataType?.propertyEditorUIAlias) return EMPTY;
					this._dataTypeData = dataType.data;
					return this._extensionRegistry?.getByAlias(dataType.propertyEditorUIAlias) ?? of(null);
				})
			),
			(manifest) => {
				if (manifest?.type === 'propertyEditorUI') {
					this._propertyEditorUIAlias = manifest.alias;
				}
			}
		);
	}

	render() {
		return html`<umb-entity-property
			label=${ifDefined(this.property?.label)}
			description=${ifDefined(this.property?.description)}
			alias="${ifDefined(this.property?.alias)}"
			property-editor-ui-alias="${ifDefined(this._propertyEditorUIAlias)}"
			.value="${this.value}"
			.config="${this._dataTypeData}"></umb-entity-property>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-node-property': UmbNodePropertyElement;
	}
}
