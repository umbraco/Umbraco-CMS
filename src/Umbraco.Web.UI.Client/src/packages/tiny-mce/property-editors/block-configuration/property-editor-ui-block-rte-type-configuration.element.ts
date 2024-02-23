import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import { UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block-type';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-property-editor-ui-block-rte-type-configuration
 */
@customElement('umb-property-editor-ui-block-rte-type-configuration')
export class UmbPropertyEditorUIBlockRteBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value: UmbBlockTypeBaseModel[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return UmbInputBlockTypeElement
			? html`<umb-input-block-type
					entity-type="block-rte-type"
					.value=${this.value}
					@change=${(e: Event) => {
						this.value = (e.target as UmbInputBlockTypeElement).value;
					}}></umb-input-block-type>`
			: '';
	}
}

export default UmbPropertyEditorUIBlockRteBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-rte-type-configuration': UmbPropertyEditorUIBlockRteBlockConfigurationElement;
	}
}
