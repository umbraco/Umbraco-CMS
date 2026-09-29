import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * @element umb-unsupported-rte-block
 */
@customElement('umb-unsupported-rte-block')
export class UmbUnsupportedRteBlockElement extends UmbLitElement {
	override render() {
		return html`
			<uui-ref-node
				standalone
				.readonly=${true}
				detail=${this.localize.term('blockEditor_unsupportedBlockDescription')}>
				<div class="selection-background" aria-hidden="true">&emsp;</div>
				<umb-icon slot="icon" name="icon-alert"></umb-icon>
				<span slot="name">${this.localize.term('blockEditor_unsupportedBlockName')}</span>
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

			/* HACK: Stretches a space character (&emsp;) to be full-width to make the RTE block appear text-selectable. [LK,NL] */
			.selection-background {
				position: absolute;
				pointer-events: none;
				font-size: 100vw;
				inset: 0;
				overflow: hidden;
				z-index: 0;
			}

			umb-icon,
			span {
				z-index: 1;
			}
		`,
	];
}

export default UmbUnsupportedRteBlockElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-unsupported-rte-block': UmbUnsupportedRteBlockElement;
	}
}
