import { UUIButtonState, UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbNotificationService } from '../../../core/services/notification';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';
import { DocumentTypeEntity } from '../../../core/mocks/data/document-type.data';
import { UmbNotificationDefaultData } from '../../../core/services/notification/layouts/default';
import { UmbDocumentTypeContext } from './document-type.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes, ManifestWithLoader } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../shared/editor-entity-layout/editor-entity-layout.element';

@customElement('umb-editor-document-type')
export class UmbEditorDocumentTypeElement extends UmbContextProviderMixin(
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

			#alias {
				padding: 0 var(--uui-size-space-3);
			}
		`,
	];

	@property()
	entityKey!: string;

	@state()
	private _documentType?: DocumentTypeEntity;

	@state()
	private _saveButtonState?: UUIButtonState;

	private _documentTypeContext?: UmbDocumentTypeContext;
	private _documentTypeStore?: UmbDocumentTypeStore;
	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this._registerExtensions();

		this.consumeAllContexts(['umbDocumentTypeStore', 'umbNotificationService'], (instances) => {
			this._documentTypeStore = instances['umbDocumentTypeStore'];
			this._notificationService = instances['umbNotificationService'];
			this._observeDocumentType();
		});

		this.provideContext('umbDocumentType', this._documentTypeContext);
	}

	private _registerExtensions() {
		const extensions: Array<ManifestWithLoader<ManifestTypes>> = [
			{
				type: 'editorView',
				alias: 'Umb.EditorView.DocumentType.Design',
				name: 'Document Type Editor Design View',
				loader: () => import('./views/design/editor-view-document-type-design.element'),
				weight: 100,
				meta: {
					editors: ['Umb.Editor.DocumentType'],
					label: 'Design',
					pathname: 'design',
					icon: 'edit',
				},
			},
		];

		extensions.forEach((extension) => {
			if (umbExtensionsRegistry.isRegistered(extension.alias)) return;
			umbExtensionsRegistry.register(extension);
		});
	}

	private _observeDocumentType() {
		if (!this._documentTypeStore) return;

		// TODO: This should be done in a better way, but for now it works.
		this.observe<DocumentTypeEntity>(this._documentTypeStore.getByKey(this.entityKey), (documentType) => {
			if (!documentType) return; // TODO: Handle nicely if there is no document type

			if (!this._documentTypeContext) {
				this._documentTypeContext = new UmbDocumentTypeContext(documentType);
				this.provideContext('umbDocumentTypeContext', this._documentTypeContext);
			} else {
				this._documentTypeContext.update(documentType);
			}

			this.observe<DocumentTypeEntity>(this._documentTypeContext.data.pipe(distinctUntilChanged()), (data) => {
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
			const data: UmbNotificationDefaultData = { message: 'Document Type Saved' };
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
				this._documentTypeContext?.update({ name: target.value });
			}
		}
	}

	render() {
		return html`
			<umb-editor-entity-layout alias="Umb.Editor.DocumentType">
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
			</umb-editor-entity-layout>
		`;
	}
}

export default UmbEditorDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-document-type': UmbEditorDocumentTypeElement;
	}
}
