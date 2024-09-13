import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/block-custom-view';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-custom-view-test')
export class UmbCustomViewTestElement extends UmbLitElement implements UmbBlockEditorCustomViewElement {
	@property({ attribute: false })
	content?: UmbBlockDataType;

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
