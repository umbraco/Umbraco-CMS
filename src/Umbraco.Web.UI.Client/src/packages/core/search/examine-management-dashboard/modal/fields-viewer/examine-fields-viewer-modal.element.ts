import type {
	UmbExamineFieldsViewerModalData,
	UmbExamineFieldsViewerModalValue,
} from './examine-fields-viewer-modal.token.js';
import { html, css, nothing, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-examine-fields-viewer-modal')
export class UmbExamineFieldsViewerModalElement extends UmbModalBaseElement<
	UmbExamineFieldsViewerModalData,
	UmbExamineFieldsViewerModalValue
> {
	private _handleClose() {
		this.modalContext?.reject();
	}

	override render() {
		if (!this.data) return nothing;

		return html`
			<umb-body-layout headline="${this.data?.name}">
				<uui-scroll-container id="field-viewer">
					<span>
						<uui-table>
							<uui-table-head>
								<uui-table-head-cell> Field </uui-table-head-cell>
								<uui-table-head-cell> Value </uui-table-head-cell>
							</uui-table-head>
							${Object.values(this.data.searchResult.fields ?? []).map((cell) => {
								return html`<uui-table-row>
									<uui-table-cell> ${cell.name} </uui-table-cell>
									<uui-table-cell> ${cell.values?.join(', ')} </uui-table-cell>
								</uui-table-row>`;
							})}
						</uui-table>
					</span>
				</uui-scroll-container>
				<div slot="actions">
					<uui-button
						look="primary"
						label=${this.localize.term('general_close')}
						@click=${this._rejectModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			:host {
				display: relative;
			}

			span {
				display: block;
				padding-right: var(--uui-size-space-5);
			}

			uui-scroll-container {
				overflow-y: scroll;
				max-height: 100%;
				min-height: 0;
			}
		`,
	];
}

export default UmbExamineFieldsViewerModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-examine-fields-viewer-modal': UmbExamineFieldsViewerModalElement;
	}
}
