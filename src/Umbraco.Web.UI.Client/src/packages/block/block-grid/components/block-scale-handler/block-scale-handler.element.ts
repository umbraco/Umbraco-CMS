import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../block-grid-block-view/index.js';

/**
 * @element umb-block-scale-handler
 */
@customElement('umb-block-scale-handler')
export class UmbBlockGridScaleHandlerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	render() {
		return html``;
	}

	static styles = [css``];
}

export default UmbBlockGridScaleHandlerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-scale-handler': UmbBlockGridScaleHandlerElement;
	}
}
