import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-editor-user-group')
export class UmbEditorUserGroupElement extends LitElement {
	render() {
		return html`<div>User Group</div>`;
	}
}

export default UmbEditorUserGroupElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-user-group': UmbEditorUserGroupElement;
	}
}
