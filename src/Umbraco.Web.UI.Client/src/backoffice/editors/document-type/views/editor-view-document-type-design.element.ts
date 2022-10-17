import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { distinctUntilChanged } from 'rxjs';
import type { DocumentTypeEntity } from '../../../../core/mocks/data/document-type.data';
import { UmbDocumentTypeContext } from '../document-type.context';
import { UmbObserverMixin } from '../../../../core/observer';
import { UmbContextConsumerMixin } from '@umbraco-cms/context-api';

@customElement('umb-editor-view-document-type-design')
export class UmbEditorViewDocumentTypeDesignElement extends UmbContextConsumerMixin(UmbObserverMixin(LitElement)) {
	static styles = [UUITextStyles, css``];

	@state()
	_documentType?: DocumentTypeEntity;

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

		this.observe<DocumentTypeEntity>(this._documentTypeContext.data.pipe(distinctUntilChanged()), (documentType) => {
			this._documentType = documentType;
		});
	}

	render() {
		return html`<div>Design of ${this._documentType?.name}</div>`;
	}
}

export default UmbEditorViewDocumentTypeDesignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-document-type-design': UmbEditorViewDocumentTypeDesignElement;
	}
}
