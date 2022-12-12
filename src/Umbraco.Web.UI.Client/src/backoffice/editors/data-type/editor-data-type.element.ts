import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbDataTypeStore } from '../../../core/stores/data-type/data-type.store';
import { UmbDataTypeContext } from './data-type.context';
import type { DataTypeDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '@umbraco-cms/context-api';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../shared/editor-entity-layout/editor-entity-layout.element';

/**
 *  @element umb-editor-data-type
 *  @description - Element for displaying a Data Type Editor
 */
@customElement('umb-editor-data-type')
export class UmbEditorDataTypeElement extends UmbContextProviderMixin(
	UmbContextConsumerMixin(UmbObserverMixin(LitElement))
) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			#header {
				/* TODO: can this be applied from layout slot CSS? */
				margin: 0 var(--uui-size-layout-1);
				flex:1 1 auto;
			}
		`,
	];

	@property({ type: String })
	entityKey = '';

	@state()
	private _dataTypeName = '';

	private _dataTypeContext?: UmbDataTypeContext;
	private _dataTypeStore?: UmbDataTypeStore;

	constructor() {
		super();

		this._registerExtensions();

		this.consumeAllContexts(['umbDataTypeStore'], (instances) => {
			this._dataTypeStore = instances['umbDataTypeStore'];
			this._observeDataType();
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	private _registerExtensions() {
		const extensions: Array<any> = [
			{
				type: 'editorView',
				alias: 'Umb.EditorView.DataType.Edit',
				name: 'Data Type Editor Edit View',
				loader: () => import('./views/edit/editor-view-data-type-edit.element'),
				weight: 90,
				meta: {
					editors: ['Umb.Editor.DataType'],
					label: 'Edit',
					pathname: 'edit',
					icon: 'edit',
				},
			},
			{
				type: 'editorView',
				alias: 'Umb.EditorView.DataType.Info',
				name: 'Data Type Editor Info View',
				loader: () => import('./views/info/editor-view-data-type-info.element'),
				weight: 90,
				meta: {
					editors: ['Umb.Editor.DataType'],
					label: 'Info',
					pathname: 'info',
					icon: 'info',
				},
			},
			{
				type: 'editorAction',
				alias: 'Umb.EditorAction.DataType.Save',
				name: 'Save Data Type Editor Action',
				loader: () => import('./actions/save/editor-action-data-type-save.element'),
				meta: {
					editors: ['Umb.Editor.DataType'],
				},
			},
		];

		extensions.forEach((extension) => {
			if (umbExtensionsRegistry.isRegistered(extension.alias)) return;
			umbExtensionsRegistry.register(extension);
		});
	}

	private _observeDataType() {
		if (!this._dataTypeStore) return;

		this.observe<DataTypeDetails>(this._dataTypeStore.getByKey(this.entityKey), (dataType) => {
			if (!dataType) return; // TODO: Handle nicely if there is no data type.

			if (!this._dataTypeContext) {
				this._dataTypeContext = new UmbDataTypeContext(dataType);
				this.provideContext('umbDataTypeContext', this._dataTypeContext);
			} else {
				this._dataTypeContext.update(dataType);
			}

			this.observe<DataTypeDetails>(this._dataTypeContext.data, (dataType) => {
				if (dataType && dataType.name !== this._dataTypeName) {
					this._dataTypeName = dataType.name ?? '';
				}
			});
		});
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;
		this._dataTypeContext?.setPropertyValue(target?.alias, target?.value);
	};

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._dataTypeContext?.update({ name: target.value });
			}
		}
	}

	render() {
		return html`
			<umb-editor-entity-layout alias="Umb.Editor.DataType">
				<uui-input id="header" slot="header" .value=${this._dataTypeName} @input="${this._handleInput}"></uui-input>
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorDataTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-data-type': UmbEditorDataTypeElement;
	}
}
