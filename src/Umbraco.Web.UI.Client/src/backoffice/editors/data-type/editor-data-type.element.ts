import { UUIButtonState, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription } from 'rxjs';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '../../../core/context';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbDataTypeStore } from '../../../core/stores/data-type/data-type.store';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';
import { UmbObserverMixin } from '../../../core/observer';
import { UmbDataTypeContext } from './data-type.context';

import '../shared/editor-entity-layout/editor-entity-layout.element';
import { DataTypeDetails } from '../../../mocks/data/data-type.data';

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

			#name {
				width: 100%;
			}
		`,
	];

	@property({ type: String })
	entityKey = '';

	@state()
	private _dataTypeName = '';

	@state()
	private _saveButtonState?: UUIButtonState;

	private _dataTypeContext?: UmbDataTypeContext;
	private _dataTypeStore?: UmbDataTypeStore;
	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeContext('umbDataTypeStore', (store: UmbDataTypeStore) => {
			this._dataTypeStore = store;
			this._observeDataType();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	private _observeDataType() {
		if (!this._dataTypeStore) return;

		this.observe(this._dataTypeStore.getByKey(this.entityKey), (dataType: DataTypeDetails) => {
			if (!dataType) return; // TODO: Handle nicely if there is no data type.

			if (!this._dataTypeContext) {
				this._dataTypeContext = new UmbDataTypeContext(dataType);
				this.provideContext('umbDataTypeContext', this._dataTypeContext);
			} else {
				this._dataTypeContext.update(dataType);
			}

			this.observe(this._dataTypeContext.data, (dataType) => {
				if (dataType && dataType.name !== this._dataTypeName) {
					this._dataTypeName = dataType.name;
				}
			});
		});
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;
		this._dataTypeContext?.setPropertyValue(target?.alias, target?.value);
	};

	private async _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._dataTypeStore || !this._dataTypeContext) return;

		try {
			this._saveButtonState = 'waiting';
			const dataType = this._dataTypeContext.getData();
			await this._dataTypeStore.save([dataType]);
			const data: UmbNotificationDefaultData = { message: 'Data Type Saved' };
			this._notificationService?.peek('positive', { data });
			this._saveButtonState = 'success';
		} catch (error) {
			this._saveButtonState = 'failed';
		}
	}

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
				<uui-input id="name" slot="name" .value=${this._dataTypeName} @input="${this._handleInput}"></uui-input>
				<!-- TODO: these could be extensions points too -->
				<div slot="actions">
					<uui-button
						@click=${this._onSave}
						look="primary"
						color="positive"
						label="Save"
						.state="${this._saveButtonState}"></uui-button>
				</div>
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
