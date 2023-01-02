import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-workspace-language-root')
export class UmbWorkspaceLanguageRootElement extends LitElement {
	render() {
		return html` <div>Language Root Workspace</div> `;
	}
}

export default UmbWorkspaceLanguageRootElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-language-root': UmbWorkspaceLanguageRootElement;
	}
}
