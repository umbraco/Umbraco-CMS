import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, state } from 'lit/decorators.js';

@customElement('umb-editor-action-users-save')
export class UmbEditorActionUsersSaveElement extends LitElement {
	static styles = [UUITextStyles, css``];

	private _handleSave() {
		console.log('save');
	}

	render() {
		return html`<uui-button @click=${this._handleSave} look="primary" color="positive" label="save"></uui-button>`;
	}
}

export default UmbEditorActionUsersSaveElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-action-users-save': UmbEditorActionUsersSaveElement;
	}
}
