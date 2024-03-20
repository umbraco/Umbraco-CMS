import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbDefaultWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

@customElement('umb-document-blueprint-root-workspace')
export class UmbDocumentBlueprintRootWorkspaceElement extends UmbLitElement {
	render() {
		return html`<div>Document Blueprint Root Workspace</div> `;
	}
}

export { UmbDocumentBlueprintRootWorkspaceElement as element };

export { UmbDefaultWorkspaceContext as api };

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-root-workspace': UmbDocumentBlueprintRootWorkspaceElement;
	}
}
