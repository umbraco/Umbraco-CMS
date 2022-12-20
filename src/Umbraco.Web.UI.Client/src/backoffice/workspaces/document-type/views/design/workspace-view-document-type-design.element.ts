import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeContext } from '../../document-type.context';
import type { DocumentTypeDetails } from '@umbraco-cms/models';
import { UmbObserverMixin } from '@umbraco-cms/observable-api';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-workspace-view-document-type-design')
export class UmbWorkspaceViewDocumentTypeDesignElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	_documentType?: DocumentTypeDetails;

	private _documentTypeContext?: UmbDocumentTypeContext;

	constructor() {
		super();

		this.consumeContext('umbDocumentTypeContext', (documentTypeContext) => {
			this._documentTypeContext = documentTypeContext;
			this._observeDocumentType();
		});
	}

	private _observeDocumentType() {
		if (!this._documentTypeContext) return;

		this.observe<DocumentTypeDetails>(this._documentTypeContext.data.pipe(distinctUntilChanged()), (documentType) => {
			this._documentType = documentType;
		});
	}

	render() {
		return html`<div>Design of ${this._documentType?.name}</div>`;
	}
}

export default UmbWorkspaceViewDocumentTypeDesignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-document-type-design': UmbWorkspaceViewDocumentTypeDesignElement;
	}
}
