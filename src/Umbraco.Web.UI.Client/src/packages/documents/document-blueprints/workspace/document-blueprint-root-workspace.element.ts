import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

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
