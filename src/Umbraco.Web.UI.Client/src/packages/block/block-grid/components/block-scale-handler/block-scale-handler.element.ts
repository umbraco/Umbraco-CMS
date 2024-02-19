import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { html, css, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/extension-registry';
import '../block-grid-block-view/index.js';

/**
 * @element umb-block-scale-handler
 */
@customElement('umb-block-scale-handler')
export class UmbBlockGridScaleHandlerElement extends UmbLitElement implements UmbPropertyEditorUiElement {
	//
	render() {
		return html`
			<div id="handler"></div>
			<div id="label">TODO: Label content [NL]</div>
		`;
	}

	static styles = [
		css`
			:host() {
				position: absolute;
				inset: 0;
				pointer-events: none;
			}

			#handler {
				pointer-events: all;
				cursor: nwse-resize;
				position: absolute;
				// TODO: Look at the feature I out-commented here, what was that supose to do [NL]:
				//display: var(--umb-block-grid--block-ui-display, block);
				display: block;
				z-index: 10;
				bottom: -4px;
				right: -4px;
				width: 8px;
				height: 8px;
				padding: 0;
				background-color: var(--uui-color-surface);
				border: var(--uui-color-interative) solid 1px;
				box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.7);
				opacity: 0;
				transition: opacity 120ms;
			}
			#handler:focus {
				opacity: 1;
			}
			#handler::after {
				content: '';
				position: absolute;
				inset: -10px;
				background-color: rgba(0, 0, 0, 0);
			}
			#handler:focus + #label,
			#handler:hover + #label {
				opacity: 1;
			}

			#label {
				position: absolute;
				display: block;
				left: 100%;
				margin-left: 6px;
				margin-top: 6px;
				z-index: 2;

				background-color: white;
				color: black;
				font-size: 12px;
				padding: 0px 6px;
				border-radius: 3px;
				opacity: 0;
				transition: opacity 120ms;

				pointer-events: none;
				white-space: nowrap;
			}

			:host([scale-mode]) > #handler {
				opacity: 1;
			}
			:host([scale-mode]) > #label {
				opacity: 1;
			}
		`,
	];
}

export default UmbBlockGridScaleHandlerElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-scale-handler': UmbBlockGridScaleHandlerElement;
	}
}
