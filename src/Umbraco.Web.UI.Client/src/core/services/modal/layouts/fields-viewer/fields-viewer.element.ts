import { html, TemplateResult, css } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbModalLayoutElement } from '../modal-layout.element';

export interface UmbModalFieldsViewerData {
	document: UmbModalDocumentData;
	values: object;
}

export interface UmbModalDocumentData {
	id: number;
	name: string;
	score: number;
}

@customElement('umb-modal-layout-fields-viewer')
export class UmbModalLayoutFieldsViewerElement extends UmbModalLayoutElement<UmbModalFieldsViewerData> {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: relative;
			}
			uui-dialog-layout {
				display: flex;
				flex-direction: column;
				height: 100%;
				background-color: white;
				box-shadow: var(--uui-shadow-depth-1, 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24));
				border-radius: var(--uui-border-radius);
				padding: var(--uui-size-space-5);
				box-sizing: border-box;
			}
			uui-scroll-container {
				line-height: 0;
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(40px, auto));
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
			}
			div {
				display: flex;
				flex-direction: row-reverse;
			}
		`,
	];

	private _isIteratable(check: any) {
		if (typeof check[Symbol.iterator] == 'function' && typeof check != 'string') return true;
		else return false;
	}

	private _handleClose() {
		this.modalHandler?.close();
	}

	render() {
		if (this.data?.values) {
			return html`
				<uui-dialog-layout class="uui-text" headline="${this.data.document.name}">
					<uui-scroll-container id="icon-selection">
						<uui-table>
							<uui-table-head>
								<uui-table-head-cell> Field </uui-table-head-cell>
								<uui-table-head-cell> Value </uui-table-head-cell>
							</uui-table-head>
							${Object.entries(this.data?.values).map((cell) => {
								return html`<uui-table-row>
									<uui-table-cell> ${cell[0]} </uui-table-cell>
									<uui-table-cell> ${JSON.stringify(cell[1]).replace(/,/g, ', ')} </uui-table-cell>
								</uui-table-row>`;
							})}
						</uui-table>
					</uui-scroll-container>
					<div>
						<uui-button look="primary" @click="${this._handleClose}">Close</uui-button>
					</div>
				</uui-dialog-layout>
			`;
		} else return html``;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-layout-fields-viewer': UmbModalLayoutFieldsViewerElement;
	}
}
