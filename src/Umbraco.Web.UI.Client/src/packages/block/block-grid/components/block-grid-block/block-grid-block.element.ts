import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

import '@umbraco-cms/backoffice/ufm';
import '../block-grid-areas-container/index.js';
import '../ref-grid-block/index.js';

/**
 * @element umb-block-grid-block
 */
@customElement('umb-block-grid-block')
export class UmbBlockGridBlockElement extends UmbLitElement {
	//
	@property({ attribute: false })
	label?: string;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	override render() {
		return html`<umb-ref-grid-block standalone href=${this.config?.editContentPath ?? ''}>
			<umb-ufm-render inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			<umb-block-grid-areas-container slot="areas"></umb-block-grid-areas-container>
		</umb-ref-grid-block>`;
	}

	static override styles = [
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}
		`,
	];
}

export default UmbBlockGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block': UmbBlockGridBlockElement;
	}
}
