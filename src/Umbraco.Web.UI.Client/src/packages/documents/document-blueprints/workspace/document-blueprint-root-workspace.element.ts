import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends UmbLitElement {
	render() {
		return html`<div>Document Blueprint Root Workspace</div> `;
	}
}

export { UmbDocumentBlueprintRootWorkspaceElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-root-workspace': UmbDocumentBlueprintRootWorkspaceElement;
	}
}
