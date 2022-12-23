import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-workspace-document-blueprint-root')
export class UmbWorkspaceDocumentBlueprintRootElement extends LitElement {
	render() {
		return html` <div>Document Blueprint Root Workspace</div> `;
	}
}

export default UmbWorkspaceDocumentBlueprintRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-document-blueprint-root': UmbWorkspaceDocumentBlueprintRootElement;
	}
}
