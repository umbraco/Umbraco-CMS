import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';

@customElement('umb-workspace-view-document-type-listview')
export class UmbWorkspaceViewDocumentTypeListviewElement extends UmbLitElement {
	static styles = [UUITextStyles, css``];

	@state()
	_documentType?: DocumentTypeResponseModel;

	private _workspaceContext?: UmbWorkspaceDocumentTypeContext;

	constructor() {
		super();

		// TODO: Figure out if this is the best way to consume the context or if it can be strongly typed with an UmbContextToken
		this.consumeContext<UmbWorkspaceDocumentTypeContext>('umbWorkspaceContext', (documentTypeContext) => {
			this._workspaceContext = documentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._workspaceContext) return;

		this.observe(this._workspaceContext.data, (documentType) => {
			this._documentType = documentType;
		});
	}

	render() {
		return html` Listview `;
	}
}

export default UmbWorkspaceViewDocumentTypeListviewElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-document-type-listview': UmbWorkspaceViewDocumentTypeListviewElement;
	}
}
