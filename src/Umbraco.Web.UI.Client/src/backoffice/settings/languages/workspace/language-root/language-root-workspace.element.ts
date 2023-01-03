import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-language-root-workspace')
export class UmbLanguageRootWorkspaceElement extends LitElement {
	render() {
		return html` <div>Language Root Workspace</div> `;
	}
}

export default UmbLanguageRootWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-root-workspace': UmbLanguageRootWorkspaceElement;
	}
}
