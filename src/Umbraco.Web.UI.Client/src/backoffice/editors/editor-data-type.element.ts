import { html, LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('umb-editor-data-type')
export class UmbEditorDataTypeElement extends LitElement {
	@property()
	id!: string;

	private _onSave() {
		console.log('SAVE DATA TYPE');
	}

	render() {
		return html`
			<umb-editor-layout>
				<uui-input slot="name" value="name"></uui-input>

				<div>Some Content Here: ${this.id}</div>

				<div slot="actions">
					<uui-button @click=${this._onSave} look="primary" color="positive" label="Save"></uui-button>
				</div>
			</umb-editor-layout>
		`;
	}
}

export default UmbEditorDataTypeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-data-type': UmbEditorDataTypeElement;
	}
}
