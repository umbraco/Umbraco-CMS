import { UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_CONTEXT } from './document-recycle-bin-workspace.context-token.js';
import { css, html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-document-recycle-bin-editor';

@customElement(elementName)
export class UmbDocumentRecycleBinWorkspaceEditorElement extends UmbLitElement {
	@property({ attribute: false })
	manifest?: ManifestWorkspace;

	#workspaceContext?: typeof UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_DOCUMENT_RECYCLE_BIN_WORKSPACE_CONTEXT, (workspaceContext) => {
			this.#workspaceContext = workspaceContext;
			debugger;
		});
	}

	render() {
		return html`<div>Document Recycle Bin</div>`;
	}

	static styles = [
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}
		`,
	];
}

export { UmbDocumentRecycleBinWorkspaceEditorElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDocumentRecycleBinWorkspaceEditorElement;
	}
}
