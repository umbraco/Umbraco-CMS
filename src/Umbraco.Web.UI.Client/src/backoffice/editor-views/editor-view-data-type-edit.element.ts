import { css, html, LitElement } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { DataTypeEntity } from '../../mocks/data/content.data';

@customElement('umb-editor-view-data-type-edit')
export class UmbEditorViewDataTypeEditElement extends LitElement {
	static styles = [UUITextStyles, css``];

	@property({ type: Object })
	dataType?: DataTypeEntity;

	render() {
		return html`<div>EDIT DATA TYPE: ${this.dataType?.id}</div> `;
	}
}

export default UmbEditorViewDataTypeEditElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-view-data-type-edit': UmbEditorViewDataTypeEditElement;
	}
}
