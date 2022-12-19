import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-workspace-language-collection')
export class UmbWorkspaceLanguageCollectionElement extends LitElement {
	render() {
		return html` <div>Language Collection Workspace</div> `;
	}
}

export default UmbWorkspaceLanguageCollectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-workspace-language-collection': UmbWorkspaceLanguageCollectionElement;
	}
}
