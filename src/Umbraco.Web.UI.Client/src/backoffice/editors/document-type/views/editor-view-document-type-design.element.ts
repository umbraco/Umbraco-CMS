import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';
import { UmbContextConsumerMixin } from '../../../../core/context';
import type { DocumentTypeEntity } from '../../../../mocks/data/document-type.data';
import { Subscription, distinctUntilChanged } from 'rxjs';
import { UmbDocumentTypeContext } from '../document-type.context';

@customElement('umb-editor-view-document-type-design')
export class UmbEditorViewDocumentTypeDesignElement extends UmbContextConsumerMixin(LitElement) {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	documentType?: DocumentTypeEntity;

	@state()
	_documentType?: DocumentTypeEntity;

	private _documentTypeContext?: UmbDocumentTypeContext;

	private _documentTypeSubscription?: Subscription;

	constructor() {
		super();

		this.consumeContext('umbDocumentType', (documentTypeContext) => {
			this._documentTypeContext = documentTypeContext;
			this._useDocumentType();
		});
	}

	private _useDocumentType() {
		this._documentTypeSubscription?.unsubscribe();

		this._documentTypeSubscription = this._documentTypeContext?.data
			.pipe(distinctUntilChanged())
			.subscribe((documentType) => {
				this._documentType = documentType;
			});
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._documentTypeSubscription?.unsubscribe();
	}

	render() {
		return html`<div>Design ${this._documentType?.name}</div>`;
	}
}

export default UmbEditorViewDocumentTypeDesignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-document-type-design': UmbEditorViewDocumentTypeDesignElement;
	}
}
