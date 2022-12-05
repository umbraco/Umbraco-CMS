import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type.store';
import { DocumentTypeEntity } from '../../../core/mocks/data/document-type.data';
import { UmbDocumentTypeContext } from './document-type.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes, ManifestWithLoader } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';
import { UmbModalService } from '@umbraco-cms/services';

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

			#icon {
				font-size: calc(var(--uui-size-layout-3) / 2);
			}
		`,
	];

	@property()
	entityKey!: string;

	@state()
	private _documentType?: DocumentTypeEntity;

	@state()
	private _icon = {
		color: '#000000',
		name: 'umb:document-dashed-line',
	};

	private _documentTypeContext?: UmbDocumentTypeContext;
	private _documentTypeStore?: UmbDocumentTypeStore;

	private _modalService?: UmbModalService;

	constructor() {
		super();

		this._registerExtensions();

		this.consumeAllContexts(['umbDocumentTypeStore', 'umbModalService'], (instances) => {
			this._documentTypeStore = instances['umbDocumentTypeStore'];
			this._modalService = instances['umbModalService'];
			this._observeDocumentType();
		});
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
			{
				type: 'editorAction',
				alias: 'Umb.EditorAction.DocumentType.Save',
				name: 'Save Document Type Editor Action',
				loader: () => import('./actions/save/editor-action-document-type-save.element'),
				meta: {
					editors: ['Umb.Editor.DocumentType'],
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

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._documentTypeContext?.update({ name: target.value });
			}
		}
	}

	private async _handleIconClick() {
		const modalHandler = this._modalService?.iconPicker();

		modalHandler?.onClose().then((saved) => {
			if (saved) this._documentTypeContext?.update({ icon: saved.icon });
			// TODO save color as well and update styling on the icon that shows up
		});
	}

	render() {
		return html`
			<umb-editor-entity-layout alias="Umb.Editor.DocumentType">
				<div slot="icon">
					<uui-button id="icon" @click=${this._handleIconClick} compact>
						<uui-icon
							name="${this._documentType?.icon || 'umb:document-dashed-line'}"
							style="color: ${this._icon.color}"></uui-icon>
					</uui-button>
				</div>

				<div slot="name">
					<uui-input id="name" .value=${this._documentType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._documentType?.alias}</div>
					</uui-input>
				</div>

				<div slot="footer">Keyboard Shortcuts</div>
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
