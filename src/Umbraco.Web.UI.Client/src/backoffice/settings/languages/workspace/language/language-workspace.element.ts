import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-language-workspace')
export class UmbLanguageWorkspaceElement extends LitElement {
	render() {
		return html` <div>Language Workspace</div> `;
	}
}

export default UmbLanguageWorkspaceElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-language-workspace': UmbLanguageWorkspaceElement;
	}
}
