import type { UmbBlockTypeBase, UmbInputBlockTypeElement } from '../../../../../block-type/index.js';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-grid-block-configuration
 */
@customElement('umb-property-editor-ui-block-grid-block-configuration')
export class UmbPropertyEditorUIBlockGridBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value: UmbBlockTypeBase[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`<umb-input-block-type
			.value=${this.value}
			@change=${(e: Event) => {
				this.value = (e.target as UmbInputBlockTypeElement).value;
			}}></umb-input-block-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockGridBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-block-configuration': UmbPropertyEditorUIBlockGridBlockConfigurationElement;
	}
}
