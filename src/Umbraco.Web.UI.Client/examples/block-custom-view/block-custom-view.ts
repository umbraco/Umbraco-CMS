import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, LitElement, property, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/block-custom-view';

// eslint-disable-next-line local-rules/enforce-umb-prefix-on-element-name
@customElement('example-block-custom-view')
// eslint-disable-next-line local-rules/umb-class-prefix
export class ExampleBlockCustomView extends UmbElementMixin(LitElement) implements UmbBlockEditorCustomViewElement {
	//
	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	override render() {
		return html`
			<div class="uui-text ${this.settings?.blockAlignment ? 'align-' + this.settings?.blockAlignment : undefined}">
				<h5 class="uui-text">My Custom View</h5>
				<p>Headline: ${this.content?.headline}</p>
				<p>Alignment: ${this.settings?.blockAlignment}</p>
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
				background-color: red;
				color: white;
				border-radius: 9px;
				padding: 12px;
			}

			.align-center {
				text-align: center;
			}
			.align-right {
				text-align: right;
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
