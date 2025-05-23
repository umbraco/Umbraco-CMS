import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { css, customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';

@customElement('umb-block-grid-block-unsupported')
export class UmbBlockGridBlockUnsupportedElement extends UmbLitElement {
	override render() {
		return html`
			<div id="host">
				<div id="open-part">
					${this.#renderBlockInfo()}
					<slot></slot>
					<slot name="tag"></slot>
				</div>
				${this.#renderInside()}
			</div>
		`;
	}

	#renderBlockInfo() {
		return html`
			<span id="content">
				<span id="icon">
					<umb-icon name="icon-alert"></umb-icon>
				</span>
				<div id="info">
					<span id="name">${this.localize.term('blockEditor_unsupportedBlockName')}</span>
				</div>
			</span>
		`;
	}

	#renderInside() {
		return html`<div id="inside" draggable="false">
			${this.localize.term('blockEditor_unsupportedBlockDescription')}
			<umb-block-grid-areas-container slot="areas"></umb-block-grid-areas-container>
		</div>`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			umb-block-grid-areas-container {
				margin-top: calc(var(--uui-size-2) + 1px);
			}
			umb-block-grid-areas-container::part(area) {
				margin: var(--uui-size-2);
			}

			#exposeButton {
				width: 100%;
				min-height: var(--uui-size-16);
			}

			#host {
				position: relative;
				display: block;
				width: 100%;

				box-sizing: border-box;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-surface);

				border: 1px solid var(--uui-color-border);
				transition: border-color 80ms;

				min-width: 250px;
			}
			#open-part + * {
				border-top: 1px solid var(--uui-color-border);
			}
			#open-part {
				cursor: default;
				transition: border-color 80ms;
			}
			#host {
				border-color: var(--uui-color-disabled-standalone);
			}

			:host([unpublished]) #open-part #content {
				opacity: 0.6;
			}

			slot[name='tag'] {
				flex-grow: 1;

				display: flex;
				justify-content: flex-end;
				align-items: center;
			}

			#content {
				align-self: stretch;
				line-height: normal;
				display: flex;
				position: relative;
				align-items: center;
			}

			#open-part {
				color: inherit;
				text-decoration: none;

				display: flex;
				text-align: left;
				align-items: center;
				justify-content: flex-start;
				width: 100%;
				border: none;
				background: none;

				min-height: var(--uui-size-16);
				padding: calc(var(--uui-size-2) + 1px);
			}

			#icon {
				font-size: 1.2em;
				margin-left: var(--uui-size-2);
				margin-right: var(--uui-size-1);
			}

			#info {
				display: flex;
				flex-direction: column;
				align-items: start;
				justify-content: center;
				height: 100%;
				padding-left: var(--uui-size-2, 6px);
			}

			#name {
				font-weight: 700;
			}

			uui-tag {
				margin-left: 0.5em;
				margin-bottom: -0.3em;
				margin-top: -0.3em;
				vertical-align: text-top;
			}

			#inside {
				position: relative;
				display: block;
				padding: calc(var(--uui-size-layout-1));
			}
		`,
	];
}

export default UmbBlockGridBlockUnsupportedElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-grid-block-unsupported': UmbBlockGridBlockUnsupportedElement;
	}
}
