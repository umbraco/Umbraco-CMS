import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type/document-type.store';
import { DocumentTypeEntity } from '../../../core/mocks/data/document-type.data';
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

	private _documentTypeContext?: UmbDocumentTypeContext;
	private _documentTypeStore?: UmbDocumentTypeStore;

	constructor() {
		super();

		this._registerExtensions();

		this.consumeContext('umbDocumentTypeStore', (instance) => {
			this._documentTypeStore = instance;
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
