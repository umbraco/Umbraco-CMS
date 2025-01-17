import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, LitElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';

@customElement('example-property-editor')
export class ExamplePropertyEditor extends UmbElementMixin(LitElement) {
	override render() {
		return html` <h1 class="uui-h2">Property Editor Example</h1> `;
	}

	static override styles = [UmbTextStyles];
}

export default ExamplePropertyEditor;

declare global {
	interface HTMLElementTagNameMap {
		'example-property-editor': ExamplePropertyEditor;
	}
}
