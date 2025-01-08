import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, nothing, property } from '@umbraco-cms/backoffice/external/lit';
import type { UmbBlockDataType } from '@umbraco-cms/backoffice/block';
import type { UmbBlockEditorCustomViewConfiguration } from '@umbraco-cms/backoffice/block-custom-view';

import '@umbraco-cms/backoffice/ufm';
import '../block-grid-areas-container/index.js';
import '../ref-grid-block/index.js';

@customElement('umb-block-grid-block')
export class UmbBlockGridBlockElement extends UmbLitElement {
	//
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

	override render() {
		return html`<umb-ref-grid-block
			standalone
			href=${(this.config?.showContentEdit ? this.config?.editContentPath : undefined) ?? ''}>
			<umb-icon slot="icon" .name=${this.icon}></umb-icon>
			<umb-ufm-render slot="name" inline .markdown=${this.label} .value=${this.content}></umb-ufm-render>
			${this.unpublished
				? html`<uui-tag slot="name" look="secondary" title=${this.localize.term('blockEditor_notExposedDescription')}
						><umb-localize key="blockEditor_notExposedLabel"></umb-localize
					></uui-tag>`
				: nothing}
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

export default UmbBlockGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block': UmbBlockGridBlockElement;
	}
}
