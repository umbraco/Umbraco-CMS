import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-stylesheets-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-stylesheets-configuration')
export class UmbPropertyEditorUITinyMceStylesheetsConfigurationElement extends UmbLitElement {
	@property()
	value: string[] = [];

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`<ul>
			${this.value?.map((v) => html`<li><uui-checkbox value=${v}>${v}</uui-checkbox></li>`)}
		</ul>`;
	}

	static styles = [
		UUITextStyles,
		css`
			ul {
				list-style: none;
				padding: 0;
				margin: 0;
			}
		`,
	];
}

export default UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-stylesheets-configuration': UmbPropertyEditorUITinyMceStylesheetsConfigurationElement;
	}
}
