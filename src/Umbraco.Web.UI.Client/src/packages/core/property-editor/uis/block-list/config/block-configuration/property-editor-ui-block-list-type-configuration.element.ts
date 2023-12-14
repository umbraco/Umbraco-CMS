import { type UmbBlockTypeBase, UmbInputBlockTypeElement } from '@umbraco-cms/backoffice/block';
import '@umbraco-cms/backoffice/block';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-list-type-configuration
 */
@customElement('umb-property-editor-ui-block-list-type-configuration')
export class UmbPropertyEditorUIBlockListBlockConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value: UmbBlockTypeBase[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`Hey
			<umb-input-block-type
				.value=${this.value}
				@change=${(e: Event) => {
					this.value = (e.target as UmbInputBlockTypeElement).value;
				}}></umb-input-block-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockListBlockConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-list-type-configuration': UmbPropertyEditorUIBlockListBlockConfigurationElement;
	}
}
