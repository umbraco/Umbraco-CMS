import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property } from 'lit/decorators.js';
import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

/**
 * @element umb-property-editor-ui-tiny-mce-toolbar-configuration
 */
@customElement('umb-property-editor-ui-tiny-mce-toolbar-configuration')
export class UmbPropertyEditorUITinyMceToolbarConfigurationElement extends UmbLitElement {
	

	@property()
	value: string[] = [];

	@property({ type: Array, attribute: false })
	public config = [];

	render() {
		return html`TODO => get config options from backend per Umbraco.Cms.Web.BackOffice.PropertyEditors.RichTextPreValueController<ul>
			${this.value.map((v) => html`<li><uui-checkbox value=${v} checked>${v}</uui-checkbox></li>`)}
		</ul>`;
	}
	
	static styles = [
		UUITextStyles,
		css`
			ul {
				list-style: none;
				padding: 0;
				margin:0;
			}
		`,
	];
}

export default UmbPropertyEditorUITinyMceToolbarConfigurationElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-ui-tiny-mce-toolbar-configuration': UmbPropertyEditorUITinyMceToolbarConfigurationElement;
	}
}
