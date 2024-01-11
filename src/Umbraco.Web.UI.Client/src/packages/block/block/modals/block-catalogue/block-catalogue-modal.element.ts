import {
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue,
	UmbBlockCatalogueView,
	UmbBlockTypeBase,
} from '@umbraco-cms/backoffice/block';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { css, html, customElement, state, repeat, ifDefined } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	@state()
	private _blocks: Array<UmbBlockTypeBase> = [
		{
			contentElementTypeKey: 'block-catalogue-currency',
			backgroundColor: '#FFD700',
			iconColor: 'black',
			icon: 'icon-coins',
			label: 'Currency',
		},
		{
			contentElementTypeKey: 'block-catalogue-niels-cup-of-coffee',
			backgroundColor: '#964B00',
			iconColor: '#FFFDD0',
			icon: 'icon-coffee',
			label: "Niels' Cup of Coffee",
		},
		{
			contentElementTypeKey: 'block-catalogue-performance',
			label: 'Performance',
			icon: 'icon-dashboard',
		},
		{
			contentElementTypeKey: 'block-catalogue-weather',
			label: 'Weather',
			icon: 'icon-snow',
		},
		{
			contentElementTypeKey: 'block-catalogue-servers',
			label: 'Servers',
			icon: 'icon-stacked-disks',
		},
	];

	@state()
	view?: UmbBlockCatalogueView;

	connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		this.view = this.data.view ?? 'createEmpty';

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
		return html`
			<umb-body-layout headline="${this.localize.term('blockEditor_addBlock')}">
				${this.#renderViews()}
				<uui-box>
					<div>${this.view === 'clipboard' ? this.#renderClipboard() : this.#renderCreateEmpty()}</div>
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

	#renderClipboard() {
		return html`Clipboard`;
	}

	#renderCreateEmpty() {
		return repeat(
			this._blocks,
			(block) => block.label,
			(block) =>
				html` <uui-card-block-type
					name=${ifDefined(block.label)}
					background=${ifDefined(block.backgroundColor)}
					style="color: ${block.iconColor}">
					<uui-icon .name=${block.icon ?? ''}></uui-icon>
				</uui-card-block-type>`,
		);
	}

	#renderViews() {
		return html`
			<uui-tab-group slot="navigation">
				<uui-tab
					label="Create Empty"
					?active=${this.view === 'createEmpty'}
					@click=${() => (this.view = 'createEmpty')}>
					Create Empty
					<uui-icon slot="icon" name="icon-add"></uui-icon>
				</uui-tab>
				<uui-tab label="Clipboard" ?active=${this.view === 'clipboard'} @click=${() => (this.view = 'clipboard')}>
					Clipboard
					<uui-icon slot="icon" name="icon-paste-in"></uui-icon>
				</uui-tab>
			</uui-tab-group>
		`;
	}

	static styles = [
		css`
			uui-box div {
				display: grid;
				gap: 1rem;
				grid-template-columns: repeat(auto-fill, minmax(min(150px, 100%), 1fr));
			}

			uui-tab-group {
				--uui-tab-divider: var(--uui-color-border);
				border-left: 1px solid var(--uui-color-border);
				border-right: 1px solid var(--uui-color-border);
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
