import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbBlockRefNameSlotMixin } from '@umbraco-cms/backoffice/block';
import type { UmbBlockDataType, UmbBlockLabelUfmValueType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

/**
 * @element umb-ref-single-block
 * @slot name - Content rendered in the block's primary label area (the `name` slot of the inner `uui-ref-node`). The expected projection is a `<umb-ufm-render>` element owned by the parent block-single entry.
 */
@customElement('umb-ref-single-block')
export class UmbRefSingleBlockElement extends UmbBlockRefNameSlotMixin(UmbLitElement) {
	//
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
		const blockValue: UmbBlockLabelUfmValueType = { ...this.content, $settings: this.settings };
		return html`
			<uui-ref-node
				standalone
				.readonly=${!(this.config?.showContentEdit ?? false)}
				.href=${this.config?.showContentEdit ? this.config?.editContentPath : undefined}>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				${this.renderNameSlot(blockValue, 'name')}
				${when(
					this.unpublished,
					() => html`
						<uui-tag slot="name" look="secondary" title=${this.localize.term('blockEditor_notExposedDescription')}>
							<umb-localize key="blockEditor_notExposedLabel"></umb-localize>
						</uui-tag>
					`,
				)}
			</uui-ref-node>
		`;
	}

	static override readonly styles = [
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
			:host([unpublished]) umb-ufm-render,
			:host([unpublished]) ::slotted([slot='name']) {
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
