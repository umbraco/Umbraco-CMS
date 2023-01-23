import { html } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { UmbLitElement } from '@umbraco-cms/context-api';

/**
 * @element umb-property-editor-ui-tags
 */
@customElement('umb-property-editor-ui-tags')
export class UmbPropertyEditorUITagsElement extends UmbLitElement {
	static styles = [UUITextStyles];

	@property()
	value = '';

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<div>umb-property-editor-ui-tags</div>`;
	}
}

export default UmbPropertyEditorUITagsElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tags': UmbPropertyEditorUITagsElement;
	}
}
