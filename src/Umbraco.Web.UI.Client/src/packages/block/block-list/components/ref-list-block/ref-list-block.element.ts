import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';

import '@umbraco-cms/backoffice/ufm';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

@customElement('umb-ref-list-block')
export class UmbRefListBlockElement extends UmbLitElement {
	//
	@property({ type: String, reflect: false })
	label?: string;

	@property({ type: String, reflect: false })
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
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${blockValue}></umb-ufm-render>
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

			umb-ufm-render {
				user-select: none;
			}

			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
			}

			@keyframes umb-icon-jiggle {
				0%,
				100% {
					transform: rotate(6deg);
				}
				50% {
					transform: rotate(-6deg);
				}
			}

			:host(.sortable) {
				umb-icon {
					animation: umb-icon-jiggle 500ms infinite ease-in-out;
				}
			}
		`,
	];
}

export default UmbRefListBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-list-block': UmbRefListBlockElement;
	}
}
