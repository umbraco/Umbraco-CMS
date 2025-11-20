import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-example-entity-content-type-condition-unique')
export class UmbWorkspaceExampleViewUniqueElement extends UmbLitElement {
	override render() {
		return html`<uui-box>
			<h3>Content Type Unique Condition Test</h3>
			<p>It appears only on documents with GUID: <strong>42d7572e-1ba1-458d-a765-95b60040c3ac</strong></p>
		</uui-box>`;
	}
}

export default UmbWorkspaceExampleViewUniqueElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-example-entity-content-type-condition-unique': UmbWorkspaceExampleViewUniqueElement;
	}
}
