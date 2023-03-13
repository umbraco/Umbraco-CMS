import { html, css, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement } from 'lit/decorators.js';
import { UmbModalBaseElement } from '@umbraco-cms/modal';
import type { SearchResultModel } from '@umbraco-cms/backend-api';

@customElement('umb-modal-element-fields-viewer')
export class UmbModalElementFieldsViewerElement extends UmbModalBaseElement<SearchResultModel & { name: string }> {
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
			}

			span {
				display: block;
				padding-right: var(--uui-size-space-5);
			}

			uui-scroll-container {
				line-height: 0;
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
			}
			div {
				margin-top: var(--uui-size-space-5);
				display: flex;
				flex-direction: row-reverse;
			}
		`,
	];

	private _handleClose() {
		this.modalHandler?.reject();
	}

	render() {
		if (!this.data) return nothing;

		return html`
			<uui-dialog-layout class="uui-text" headline="${this.data.name}">
				<uui-scroll-container id="field-viewer">
					<span>
						<uui-table>
							<uui-table-head>
								<uui-table-head-cell> Field </uui-table-head-cell>
								<uui-table-head-cell> Value </uui-table-head-cell>
							</uui-table-head>
							${Object.values(this.data.fields ?? []).map((cell) => {
								return html`<uui-table-row>
									<uui-table-cell> ${cell.name} </uui-table-cell>
									<uui-table-cell> ${cell.values?.join(', ')} </uui-table-cell>
								</uui-table-row>`;
							})}
						</uui-table>
					</span>
				</uui-scroll-container>
				<div>
					<uui-button look="primary" @click="${this._handleClose}">Close</uui-button>
				</div>
			</uui-dialog-layout>
		`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-modal-element-fields-viewer': UmbModalElementFieldsViewerElement;
	}
}
