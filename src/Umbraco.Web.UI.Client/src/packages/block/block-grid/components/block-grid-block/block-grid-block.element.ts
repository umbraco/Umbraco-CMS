import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from '../../context/block-grid-entry.context-token.js';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
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
	#context?: typeof UMB_BLOCK_GRID_ENTRY_CONTEXT.TYPE;

	@property({ attribute: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@state()
	_ownerContentTypeName?: string;

	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_GRID_ENTRY_CONTEXT, (context) => {
			this.#context = context;
			this.observe(context.contentElementTypeName, (name) => {
				this._ownerContentTypeName = name;
			});
		});
	}

	#expose = () => {
		this.#context?.expose();
	};

	override render() {
		return html`<umb-ref-grid-block
			standalone
			href=${(this.config?.showContentEdit ? this.config?.editContentPath : undefined) ?? ''}>
			${this.config?.showContentEdit === false && this.unpublished
				? html`
						<uui-button slot="action" @click=${this.#expose}
							><uui-icon name="icon-add"></uui-icon>
							<umb-localize key="blockEditor_createThisFor" .args=${[this._ownerContentTypeName]}></umb-localize
						></uui-button>
					`
				: nothing}
			<umb-icon slot="icon" .name=${this.icon}></umb-icon>
			<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
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
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
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
