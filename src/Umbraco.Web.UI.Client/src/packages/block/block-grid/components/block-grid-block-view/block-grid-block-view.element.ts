import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import '../block-grid-areas-container/index.js';
import '../ref-grid-block/index.js';
import type { UmbBlockViewUrlsPropType } from '@umbraco-cms/backoffice/block';

/**
 * @element umb-block-grid-block
 */
@customElement('umb-block-grid-block')
export class UmbRefGridBlockElement extends UmbLitElement {
	//
	@property({ attribute: false })
	label?: string;

	@property({ attribute: false })
	urls?: UmbBlockViewUrlsPropType;

	render() {
		return html`<umb-ref-grid-block standalone .name=${this.label ?? ''} href=${this.urls?.editContent ?? ''}>
			<umb-block-grid-areas-container slot="areas"></umb-block-grid-areas-container>
		</umb-ref-grid-block>`;
	}

	static styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}
		`,
	];
}

export default UmbRefGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block': UmbRefGridBlockElement;
	}
}
