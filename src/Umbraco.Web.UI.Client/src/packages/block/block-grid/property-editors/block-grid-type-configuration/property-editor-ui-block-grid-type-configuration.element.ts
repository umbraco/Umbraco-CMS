import type { UmbBlockTypeBaseModel, UmbInputBlockTypeElement } from '../../../block-type/index.js';
import '../../../block-type/components/input-block-type/index.js';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import { html, customElement, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-block-grid-type-configuration
 */
@customElement('umb-property-editor-ui-block-grid-type-configuration')
export class UmbPropertyEditorUIBlockGridTypeConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ attribute: false })
	value: UmbBlockTypeBaseModel[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	render() {
		return html`<umb-input-block-type
			entity-type="block-grid-type"
			.value=${this.value}
			@change=${(e: Event) => {
				this.value = (e.target as UmbInputBlockTypeElement).value;
			}}></umb-input-block-type>`;
	}

	static styles = [UmbTextStyles];
}

export default UmbPropertyEditorUIBlockGridTypeConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-block-grid-type-configuration': UmbPropertyEditorUIBlockGridTypeConfigurationElement;
	}
}
