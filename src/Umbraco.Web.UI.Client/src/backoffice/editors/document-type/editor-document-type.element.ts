import { UUIButtonState, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged, Subscription } from 'rxjs';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbNotificationService } from '../../../core/services/notification.service';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';
import { DocumentTypeEntity } from '../../../mocks/data/document-type.data';
import { UmbDocumentTypeContext } from './document-type.context';

import '../shared/editor-entity/editor-entity.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/editor-view-document-type-design.element';

@customElement('umb-editor-document-type')
export class UmbEditorDocumentTypeElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
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

			#alias {
				padding: 0 var(--uui-size-space-3);
			}
		`,
	];

	@property()
	id!: string;

	@state()
	private _documentType?: DocumentTypeEntity;

	@state()
	private _saveButtonState?: UUIButtonState;

	private _documentTypeContext?: UmbDocumentTypeContext;
	private _documentTypeContextSubscription?: Subscription;

	private _documentTypeStore?: UmbDocumentTypeStore;
	private _documentTypeStoreSubscription?: Subscription;

	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeContext('umbDocumentTypeStore', (store: UmbDocumentTypeStore) => {
			this._documentTypeStore = store;
			this._useDocumentType();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		this.provideContext('umbDocumentType', this._documentTypeContext);
	}

	private _useDocumentType() {
		this._documentTypeStoreSubscription?.unsubscribe();

		// TODO: This should be done in a better way, but for now it works.
		this._documentTypeStoreSubscription = this._documentTypeStore
			?.getById(parseInt(this.id))
			.subscribe((documentType) => {
				if (!documentType) return; // TODO: Handle nicely if there is no document type

				this._documentTypeContextSubscription?.unsubscribe();

				if (!this._documentTypeContext) {
					this._documentTypeContext = new UmbDocumentTypeContext(documentType);
					this.provideContext('umbDocumentTypeContext', this._documentTypeContext);
				} else {
					this._documentTypeContext.update(documentType);
				}

				this._documentTypeContextSubscription = this._documentTypeContext.data
					.pipe(distinctUntilChanged())
					.subscribe((data) => {
						this._documentType = data;
					});
			});
	}

	private async _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (!this._documentTypeStore) return;
		if (!this._documentType) return;

		try {
			this._saveButtonState = 'waiting';
			await this._documentTypeStore.save([this._documentType]);
			this._notificationService?.peek('Data Type saved');
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
				this._documentTypeContext?.update({ name: target.value });
			}
		}
	}

	render() {
		return html`
			<umb-editor-entity alias="Umb.Editor.DocumentType">
				<div slot="icon">Icon</div>

				<div slot="name">
					<uui-input id="name" .value=${this._documentType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._documentType?.alias}</div>
					</uui-input>
				</div>

				<div slot="footer">Keyboard Shortcuts</div>

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
		`;
	}
}

export default UmbEditorDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-document-type': UmbEditorDocumentTypeElement;
	}
}
