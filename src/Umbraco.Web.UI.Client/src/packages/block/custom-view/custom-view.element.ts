import type { UmbBlockDataModel, UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/extension-registry';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-custom-view-test')
export class UmbCustomViewTestElement extends UmbLitElement implements UmbBlockEditorCustomViewElement {
	@property({ attribute: false })
	content?: UmbBlockDataModel;

	protected override render() {
		return html` Hello ${this.content?.headline} `;
	}

	static override styles = [
		css`
			:host {
				display: block;
				height: 100%;
				box-sizing: border-box;
				background-color: #dddddd;
				border-radius: 9px;
				padding: 12px;
			}
		`,
	];
}

export { UmbCustomViewTestElement as element };
