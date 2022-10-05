import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-editor-user-groups')
export class UmbEditorUserGroupsElement extends LitElement {
	render() {
		return html`<div>User Groups</div>`;
	}
}

export default UmbEditorUserGroupsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-user-groups': UmbEditorUserGroupsElement;
	}
}
