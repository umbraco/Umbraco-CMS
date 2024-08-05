import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, LitElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbBlockDataType, UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/extension-registry';

@customElement('example-block-custom-view')
export class ExampleBlockCustomView extends UmbElementMixin(LitElement) implements UmbBlockEditorCustomViewElement {
	//
	@property({ attribute: false })
	content?: UmbBlockDataType;

	override render() {
		return html`
			<div class="uui-text">
				<h5 class="uui-text">My Custom View</h5>
				<p>Headline: ${this.content.headline}</p>
			</div>
		`;
	}

	static override styles = [
		UmbTextStyles,
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

export default ExampleBlockCustomView;

declare global {
	interface HTMLElementTagNameMap {
		'example-block-custom-view': ExampleBlockCustomView;
	}
}
