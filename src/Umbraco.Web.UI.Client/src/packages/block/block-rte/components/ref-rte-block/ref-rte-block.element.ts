import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

/**
 * @element umb-ref-rte-block
 */
@customElement('umb-ref-rte-block')
export class UmbRefRteBlockElement extends UmbLitElement {
	//
	@property({ type: String })
	label?: string;

	@property({ type: String })
	icon?: string;

	@property({ type: Number, attribute: false })
	index?: number;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	override render() {
		const blockValue = { ...this.content, $settings: this.settings, $index: this.index };
		return html`
			<uui-ref-node
				standalone
				.readonly=${!(this.config?.showContentEdit ?? false)}
				.href=${this.config?.showContentEdit ? this.config?.editContentPath : undefined}>
				<div class="selection-background" aria-hidden="true">&emsp;</div>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${blockValue}></umb-ufm-render>
			</uui-ref-node>
		`;
	}

	static override readonly styles = [
		css`
			:host {
				display: block;
			}

			uui-ref-node {
				min-height: var(--uui-size-16);
			}

			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
			}

			/* HACK: Stretches a space character (&emsp;) to be full-width to make the RTE block appear text-selectable. [LK,NL] */
			.selection-background {
				position: absolute;
				pointer-events: none;
				font-size: 100vw;
				inset: 0;
				overflow: hidden;
				z-index: 0;
			}

			umb-ufm-render {
				user-select: none;
			}

			umb-icon,
			umb-ufm-render {
				z-index: 1;
			}
		`,
	];
}

export default UmbRefRteBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-rte-block': UmbRefRteBlockElement;
	}
}
