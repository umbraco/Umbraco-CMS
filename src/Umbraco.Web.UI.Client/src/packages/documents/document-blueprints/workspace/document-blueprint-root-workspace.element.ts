import { html, LitElement, customElement } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends LitElement {
	render() {
		return html`<div>Document Blueprint Root Workspace</div> `;
	}
}

export default UmbDocumentBlueprintRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-root-workspace': UmbDocumentBlueprintRootWorkspaceElement;
	}
}
