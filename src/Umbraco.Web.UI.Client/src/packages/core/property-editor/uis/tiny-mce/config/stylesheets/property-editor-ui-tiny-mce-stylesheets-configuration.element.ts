import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import {
	UmbPropertyEditorConfigCollection,
	UmbPropertyValueChangeEvent,
} from '@umbraco-cms/backoffice/property-editor';
import { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';

/**
 * @element umb-property-editor-ui-tiny-mce-stylesheets-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-stylesheets-configuration')
export class UmbPropertyEditorUITinyMceStylesheetsConfigurationElement
	extends UmbLitElement
	implements UmbPropertyEditorUiElement
{
	@property({ type: Array })
	value: string[] = [];

	@property({ type: Object, attribute: false })
	public config?: UmbPropertyEditorConfigCollection;

	#onChange(event: CustomEvent) {
		debugger;
		const checkbox = event.target as HTMLInputElement;

		if (checkbox.checked) {
			if (this.value) {
				this.value = [...this.value, checkbox.value];
			} else {
				this.value = [checkbox.value];
			}
		} else {
			this.value = this.value.filter((v) => v !== checkbox.value);
		}

		this.dispatchEvent(new UmbPropertyValueChangeEvent());
	}

	render() {
		return html`<umb-stylesheet-input></umb-stylesheet-input>`;
	}

	static styles = [UmbTextStyles, css``];
}

export default UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-stylesheets-configuration': UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;
	}
}
