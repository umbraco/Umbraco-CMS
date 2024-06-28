import type { UmbBlockDataType, UmbBlockEditorCustomViewElement } from "@umbraco-cms/backoffice/extension-registry";
import { customElement, html, property } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement } from "@umbraco-cms/backoffice/lit-element";

@customElement('umb-custom-view-test')
export class UmbCustomViewTestElement extends UmbLitElement implements UmbBlockEditorCustomViewElement {

	@property({attribute: false})
	content?: UmbBlockDataType;

	protected override render() {
		return html`
			Hello ${this.content?.headline}
		`
	}
}

export {UmbCustomViewTestElement as element};
