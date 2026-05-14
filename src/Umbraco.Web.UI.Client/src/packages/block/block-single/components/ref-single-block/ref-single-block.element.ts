import { UMB_BLOCK_SINGLE_ENTRY_CONTEXT } from '../../context/index.js';
import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';
import type { UmbUfmResolvedEvent } from '@umbraco-cms/backoffice/ufm';

@customElement('umb-ref-single-block')
export class UmbRefSingleBlockElement extends UmbLitElement {
	//
	#blockContext?: typeof UMB_BLOCK_SINGLE_ENTRY_CONTEXT.TYPE;

	constructor() {
		super();
		this.consumeContext(UMB_BLOCK_SINGLE_ENTRY_CONTEXT, (blockContext) => {
			this.#blockContext = blockContext;
		});
	}

	#onUfmResolved = (event: UmbUfmResolvedEvent) => {
		this.#blockContext?.setName(event.detail.text);
	};

	@property({ type: String, reflect: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	override render() {
		const blockValue = { ...this.content, $settings: this.settings };
		return html`
			<uui-ref-node
				standalone
				.readonly=${!(this.config?.showContentEdit ?? false)}
				.href=${this.config?.showContentEdit ? this.config?.editContentPath : undefined}>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render
					slot="name"
					inline
					.markdown=${this.label}
					.value=${blockValue}
					@umb-ufm-resolved=${this.#onUfmResolved}>
				</umb-ufm-render>
				${when(
					this.unpublished,
					() =>
						html`<uui-tag slot="name" look="secondary" title=${this.localize.term('blockEditor_notExposedDescription')}
							><umb-localize key="blockEditor_notExposedLabel"></umb-localize
						></uui-tag>`,
				)}
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
			uui-tag {
				margin-left: 0.5em;
				margin-bottom: -0.3em;
				margin-top: -0.3em;
				vertical-align: text-top;
			}
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
			}
		`,
	];
}

export default UmbRefSingleBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-single-block': UmbRefSingleBlockElement;
	}
}
