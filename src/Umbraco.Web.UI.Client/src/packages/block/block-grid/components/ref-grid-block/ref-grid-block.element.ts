import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbBlockViewUrlsPropType } from '@umbraco-cms/backoffice/block';
import { css, customElement, html, property } from '@umbraco-cms/backoffice/external/lit';
import '../block-grid-areas-container/index.js';

/**
 * @element umb-ref-grid-block
 */
@customElement('umb-ref-grid-block')
export class UmbRefGridBlockElement extends UmbLitElement {
	//
	@property({ attribute: false })
	label?: string;

	@property({ attribute: false })
	urls?: UmbBlockViewUrlsPropType;

	render() {
		return html`<uui-ref-node standalone .name=${this.label ?? ''} href=${this.urls?.editContent ?? ''}>
			<umb-block-grid-area-container></umb-block-grid-area-container>
		</uui-ref-node>`;
	}

	static styles = [
		css`
			uui-ref-node {
				min-height: var(--uui-size-16);
			}
		`,
	];
}

export default UmbRefGridBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-ref-grid-block': UmbRefGridBlockElement;
	}
}
