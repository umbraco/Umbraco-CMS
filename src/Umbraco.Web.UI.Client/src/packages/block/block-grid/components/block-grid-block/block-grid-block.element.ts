import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../../context/block-grid-entry.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';

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

	@state()
	_content?: UmbBlockDataModel;

	constructor() {
		super();

		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (context) => {
			this.observe(
				context.content,
				(content) => {
					this._content = content;
				},
				'observeContent',
			);
		});
	}

	override render() {
		return html`<umb-ref-grid-block standalone href=${this.config?.editContentPath ?? ''}>
			<umb-ufm-render inline .markdown=${this.label} .value=${this._content}></umb-ufm-render>
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
