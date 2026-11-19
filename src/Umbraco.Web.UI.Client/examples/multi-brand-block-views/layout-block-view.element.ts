import { exampleBlockViewStyles } from './block-view.styles.js';
import { css, customElement, html, LitElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block-grid';
import type { UmbBlockEditorCustomViewElement } from '@umbraco-cms/backoffice/block-custom-view';

@customElement('example-layout-block-view')
export class ExampleLayoutBlockView
	extends UmbElementMixin(LitElement)
	implements UmbBlockEditorCustomViewElement<UmbBlockGridLayoutModel>
{
	@property({ attribute: false })
	layout?: UmbBlockGridLayoutModel;

	override render() {
		return html`
			<div class="layout">
				<umb-block-grid-areas-container draggable="false"></umb-block-grid-areas-container>
			</div>
		`;
	}

	static override styles = [
		exampleBlockViewStyles,
		css`
			:host {
				overflow: visible;
			}

			.layout {
				padding: var(--uui-size-space-5) var(--uui-size-layout-1) var(--uui-size-layout-1);
			}

			umb-block-grid-areas-container {
				margin-top: var(--uui-size-space-4);
			}

			umb-block-grid-areas-container::part(area-container) {
				gap: var(--uui-size-layout-1);
			}

			umb-block-grid-areas-container::part(area) {
				padding: var(--uui-size-space-4);
			}
		`,
	];
}

export default ExampleLayoutBlockView;

declare global {
	interface HTMLElementTagNameMap {
		'example-layout-block-view': ExampleLayoutBlockView;
	}
}
