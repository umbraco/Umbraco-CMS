import { UUIButtonState, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement, nothing } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { UmbContextProviderMixin, UmbContextConsumerMixin } from '../../../core/context';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbDataTypeStore } from '../../../core/stores/data-type.store';
import { DataTypeEntity } from '../../../mocks/data/data-type.data';
import { UmbDataTypeContext } from './data-type.context';

import '../shared/editor-entity/editor-entity.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/editor-view-data-type-edit.element';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';

@customElement('umb-editor-data-type')
export class UmbEditorDataTypeElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
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

	@property()
	id!: string;

	@state()
	private _dataType?: DataTypeEntity;

	@state()
	private _saveButtonState?: UUIButtonState;

	private _dataTypeContext?: UmbDataTypeContext;
	private _dataTypeContextSubscription?: Subscription;

	private _dataTypeStore?: UmbDataTypeStore;
	private _dataTypeStoreSubscription?: Subscription;

	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeContext('umbDataTypeStore', (store: UmbDataTypeStore) => {
			this._dataTypeStore = store;
			this._useDataType();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		this.provideContext('umbDataType', this._dataTypeContext);
	}

	private _useDataType() {
		this._dataTypeStoreSubscription?.unsubscribe();

		// TODO: This should be done in a better way, but for now it works.
		this._dataTypeStoreSubscription = this._dataTypeStore?.getById(parseInt(this.id)).subscribe((dataType) => {
			if (!dataType) return; // TODO: Handle nicely if there is no node.

			this._dataTypeContextSubscription?.unsubscribe();

			if (!this._dataTypeContext) {
				this._dataTypeContext = new UmbDataTypeContext(dataType);
				this.provideContext('umbDataTypeContext', this._dataTypeContext);
			} else {
				this._dataTypeContext.update(dataType);
			}

			this._dataTypeContextSubscription = this._dataTypeContext.data.pipe(distinctUntilChanged()).subscribe((data) => {
				this._dataType = data;
			});
		});
	}

	private async _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._dataTypeStore) return;
		if (!this._dataType) return;

		try {
			this._saveButtonState = 'waiting';
			await this._dataTypeStore.save([this._dataType]);
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

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._dataTypeStoreSubscription?.unsubscribe();
		this._dataTypeContextSubscription?.unsubscribe();
	}

	render() {
		return html`
			${this._dataType
				? html`
						<umb-editor-entity alias="Umb.Editor.DataType">
							<uui-input id="name" slot="name" .value=${this._dataType?.name} @input="${this._handleInput}"></uui-input>
							<!-- TODO: these could be extensions points too -->
							<div slot="actions">
								<uui-button
									@click=${this._onSave}
									look="primary"
									color="positive"
									label="Save"
									.state="${this._saveButtonState}"></uui-button>
							</div>
						</umb-editor-entity>
				  `
				: nothing}
		`;
	}
}

export default UmbEditorDataTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-data-type': UmbEditorDataTypeElement;
	}
}
