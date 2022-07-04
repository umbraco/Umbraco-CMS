import { html, LitElement } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('umb-editor-data-type')
export class UmbEditorDataType extends LitElement {
	render() {
		return html`<div>Data types</div>`;
	}
}

export default UmbEditorDataType;
