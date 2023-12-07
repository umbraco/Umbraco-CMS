import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from 'src/packages/core/property/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-collection-view-order-by
 */
@customElement('umb-property-editor-ui-collection-view-order-by')
export class UmbPropertyEditorUICollectionViewOrderByElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property()
	value = '';

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`<div>umb-property-editor-ui-collection-view-order-by</div>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUICollectionViewOrderByElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-collection-view-order-by': UmbPropertyEditorUICollectionViewOrderByElement;
	}
}
