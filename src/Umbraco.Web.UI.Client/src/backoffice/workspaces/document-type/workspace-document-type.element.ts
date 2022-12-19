import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeStore } from '../../../core/stores/document-type/document-type.store';
import { UmbDocumentTypeContext } from './document-type.context';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '@umbraco-cms/context-api';
import type { ManifestTypes, DocumentTypeDetails } from '@umbraco-cms/models';
import { umbExtensionsRegistry } from '@umbraco-cms/extensions-registry';

import '../../property-editor-uis/icon-picker/property-editor-ui-icon-picker.element';

@customElement('umb-workspace-document-type')
export class UmbWorkspaceDocumentTypeElement extends UmbContextProviderMixin(
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
				display: flex;
				flex: 1 1 auto;
				margin: 0 var(--uui-size-layout-1);
			}

			#name {
				width: 100%;
				flex: 1 1 auto;
			}

			#alias {
				padding: 0 var(--uui-size-space-3);
			}
		`,
	];

	@property()
	entityKey!: string;

	@state()
	private _documentType?: DocumentTypeDetails;

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
		const extensions: Array<ManifestTypes> = [
			{
				type: 'workspaceView',
				alias: 'Umb.WorkspaceView.DocumentType.Design',
				name: 'Document Type Workspace Design View',
				loader: () => import('./views/design/workspace-view-document-type-design.element'),
				weight: 100,
				meta: {
					workspaces: ['Umb.Workspace.DocumentType'],
					label: 'Design',
					pathname: 'design',
					icon: 'edit',
				},
			},
			{
				type: 'workspaceAction',
				alias: 'Umb.WorkspaceAction.DocumentType.Save',
				name: 'Save Document Type Workspace Action',
				loader: () => import('./actions/save/workspace-action-document-type-save.element'),
				meta: {
					workspaces: ['Umb.Workspace.DocumentType'],
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
		this.observe<DocumentTypeDetails>(this._documentTypeStore.getByKey(this.entityKey), (documentType) => {
			if (!documentType) return; // TODO: Handle nicely if there is no document type

			if (!this._documentTypeContext) {
				this._documentTypeContext = new UmbDocumentTypeContext(documentType);
				this.provideContext('umbDocumentTypeContext', this._documentTypeContext);
			} else {
				this._documentTypeContext.update(documentType);
			}

			this.observe<DocumentTypeDetails>(this._documentTypeContext.data.pipe(distinctUntilChanged()), (data) => {
				this._documentType = data;
			});
		});
	}

	// TODO. find a way where we don't have to do this for all workspaces.
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
			<umb-workspace-entity-layout alias="Umb.Workspace.DocumentType">
				<div id="header" slot="header">
					<umb-property-editor-ui-icon-picker></umb-property-editor-ui-icon-picker>
					<uui-input id="name" .value=${this._documentType?.name} @input="${this._handleInput}">
						<div id="alias" slot="append">${this._documentType?.alias}</div>
					</uui-input>
				</div>

				<div slot="footer">Keyboard Shortcuts</div>
			</umb-workspace-entity-layout>
		`;
	}
}

export default UmbWorkspaceDocumentTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-document-type': UmbWorkspaceDocumentTypeElement;
	}
}
