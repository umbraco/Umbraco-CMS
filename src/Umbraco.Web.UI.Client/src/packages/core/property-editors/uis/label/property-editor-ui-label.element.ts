import { html } from '@umbraco-cms/backoffice/external/lit';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import type { UmbDataTypePropertyCollection } from '@umbraco-cms/backoffice/components';

/**
 * @element umb-property-editor-ui-label
 */
@customElement('umb-property-editor-ui-label')
export class UmbPropertyEditorUILabelElement extends UmbLitElement implements UmbPropertyEditorExtensionElement {
	@property()
	value = '';

	@property()
	description = '';

	@property({ type: Array, attribute: false })
	public config?: UmbDataTypePropertyCollection;

	render() {
		return html`${this.value}`;
	}

	static styles = [UUITextStyles];
}

export default UmbPropertyEditorUILabelElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-label': UmbPropertyEditorUILabelElement;
	}
}
