import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
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

	@property({ type: Boolean, reflect: true })
	unpublished?: boolean;

	@property({ attribute: false })
	content?: UmbBlockDataType;

	@property({ attribute: false })
	config?: UmbBlockEditorCustomViewConfiguration;

	override render() {
		return html`
			<uui-ref-node standalone href=${(this.config?.showContentEdit ? this.config?.editContentPath : undefined) ?? ''}>
				<umb-icon slot="icon" .name=${this.icon}></umb-icon>
				<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			</uui-ref-node>
		`;
	}

	static override styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
			:host([unpublished]) umb-icon,
			:host([unpublished]) umb-ufm-render {
				opacity: 0.6;
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
