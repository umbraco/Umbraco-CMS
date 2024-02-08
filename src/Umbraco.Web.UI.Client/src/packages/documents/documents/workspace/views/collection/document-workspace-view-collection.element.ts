import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import type { UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-document-workspace-view-collection')
export class UmbDocumentWorkspaceViewCollectionElement extends UmbLitElement implements UmbWorkspaceViewElement {
	#workspaceContext?: typeof UMB_DOCUMENT_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;

			// TODO: [LK] Get the `dataTypeId` and get the configuration for the collection view.
		});
	}

	render() {
		return html`<umb-collection alias="Umb.Collection.Document"></umb-collection>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbDocumentWorkspaceViewCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-workspace-view-collection': UmbDocumentWorkspaceViewCollectionElement;
	}
}
