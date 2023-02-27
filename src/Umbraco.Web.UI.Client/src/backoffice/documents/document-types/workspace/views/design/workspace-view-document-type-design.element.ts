import { css, html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';
import { UmbWorkspaceDocumentTypeContext } from '../../document-type-workspace.context';
import { UmbLitElement } from '@umbraco-cms/element';
import type { DocumentTypeModel } from '@umbraco-cms/backend-api';

@customElement('umb-workspace-view-document-type-design')
export class UmbWorkspaceViewDocumentTypeDesignElement extends UmbLitElement {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				padding: var(--uui-size-space-6);
			}
		`,
	];

	@state()
	_documentType?: DocumentTypeModel;

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
		return html`Design of ${this._documentType?.name}`;
	}
}

export default UmbWorkspaceViewDocumentTypeDesignElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-view-document-type-design': UmbWorkspaceViewDocumentTypeDesignElement;
	}
}
