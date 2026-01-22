import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property, when } from '@umbraco-cms/backoffice/external/lit';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

import '@umbraco-cms/backoffice/ufm';

@customElement('umb-block-grid-block')
export class UmbBlockGridBlockElement extends UmbLitElement {
	//
	@property({ attribute: false })
	label?: string;

	@property({ type: String, reflect: false })
	icon?: string;

	@property({ type: Number, attribute: false })
	index?: number;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	settings?: UmbBlockDataType;

	override render() {
		const blockValue = { ...this.content, $settings: this.settings, $index: this.index };
		return html`
			<umb-ref-grid-block
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
				<umb-block-grid-areas-container slot="areas" draggable="false"></umb-block-grid-areas-container>
			</umb-ref-grid-block>
		`;
	}

	static override styles = [
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}

			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}

			umb-ufm-render {
				user-select: none;
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

export default UmbBlockGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block': UmbBlockGridBlockElement;
	}
}
