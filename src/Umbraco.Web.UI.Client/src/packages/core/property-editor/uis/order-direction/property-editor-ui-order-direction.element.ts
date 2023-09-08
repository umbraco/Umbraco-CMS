import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
import { UmbPropertyEditorExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * @element umb-property-editor-ui-order-direction
 */
@customElement('umb-property-editor-ui-order-direction')
export class UmbPropertyEditorUIOrderDirectionElement
	extends UmbLitElement
	implements UmbPropertyEditorExtensionElement
{
	@property()
	value = '';

	@property({ attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`<div>umb-property-editor-ui-order-direction</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIOrderDirectionElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-order-direction': UmbPropertyEditorUIOrderDirectionElement;
	}
}
