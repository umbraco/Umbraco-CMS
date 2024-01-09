import { UmbBlockTypeBase } from '@umbraco-cms/backoffice/block';
import { DATA_TYPE_DETAIL_REPOSITORY_ALIAS, UmbDataTypeDetailRepository } from '@umbraco-cms/backoffice/data-type';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import {
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue,
	UmbModalBaseElement,
} from '@umbraco-cms/backoffice/modal';

type UmbBlockTypeModel = UmbBlockTypeBase & Partial<UmbDocumentTypeItemModel>;

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	@state()
	private _blocks: Array<UmbBlockTypeModel> = [
		{
			contentElementTypeKey: 'something',
			backgroundColor: 'cyan',
			iconColor: 'black',
			name: 'Mock Block',
			label: 'Mock Block',
		},
		{
			contentElementTypeKey: 'something2',
			name: 'Mock Block 2',
			backgroundColor: 'cyan',
			iconColor: 'black',
			icon: 'icon-wand',
		},
		{
			contentElementTypeKey: 'something3',
			label: 'Mock Block 3',
			backgroundColor: 'cyan',
			iconColor: 'black',
		},
	];

	connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		if (this.modalContext) {
			this.observe(this.modalContext.value, (value) => {
				//
			});
		}

		this.consumeContext(UMB_DOCUMENT_WORKSPACE_CONTEXT, (context) => {
			console.log(context);
		});
	}

	#partialUpdateBlock(linkObject: any) {
		//this.modalContext?.updateValue({ link: { ...this._link, ...linkObject } });
	}

	render() {
		// Create Empty tab
		return html`
			<umb-body-layout headline="${this.localize.term('blockEditor_addBlock')}">
				<uui-box>
					<div>
						${repeat(
							this._blocks,
							(block) => block.label,
							(block) =>
								html` <uui-card-block-type
									name=${ifDefined(block.label)}
									background=${ifDefined(block.backgroundColor)}
									style="color: ${block.iconColor}">
									<uui-icon .name=${block.icon ?? ''}></uui-icon>
								</uui-card-block-type>`,
						)}
					</div>
				</uui-box>
				<div slot="actions">
					<uui-button label=${this.localize.term('general_close')} @click=${this._rejectModal}></uui-button>
					<uui-button
						label=${this.localize.term('general_submit')}
						look="primary"
						color="positive"
						@click=${this._submitModal}></uui-button>
				</div>
			</umb-body-layout>
		`;
	}

	static styles = [
		css`
			uui-box div {
				display: grid;
				gap: 1rem;
				grid-template-columns: repeat(auto-fill, minmax(min(119px, 100%), 1fr));
			}
		`,
	];
}

export default UmbBlockCatalogueModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-catalogue-modal': UmbBlockCatalogueModalElement;
	}
}
