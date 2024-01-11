import {
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue,
	UmbBlockCatalogueView,
	UmbBlockTypeWithGroupKey,
} from '@umbraco-cms/backoffice/block';
import { UMB_DOCUMENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/document';
import { css, html, customElement, state, repeat, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import { groupBy } from '@umbraco-cms/backoffice/external/lodash';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	@state()
	private _blocks: Array<UmbBlockTypeWithGroupKey> = [];

	@state()
	private _blockGroups: Array<{ key: string; name: string }> = [];

	@state()
	view?: UmbBlockCatalogueView;

	connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		this.view = this.data.view ?? 'createEmpty';
		this._blocks = this.data.blocks ?? [];
		this._blockGroups = this.data.blockGroups ?? [];

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
				<uui-box> ${this.view === 'clipboard' ? this.#renderClipboard() : this.#renderCreateEmpty()} </uui-box>
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
		const blockArrays = groupBy(this._blocks, 'groupKey');

		const mappedGroupsAndBlocks = Object.entries(blockArrays).map(([key, value]) => {
			const group = this._blockGroups.find((group) => group.key === key);
			return { name: group?.name, blocks: value };
		});

		return html`
			${mappedGroupsAndBlocks.map(
				(group) => html`
					${group.name ? html`<h2>${group.name}</h2>` : nothing}
					<div class="blockGroup">
						${repeat(
							group.blocks,
							(block) => block.contentElementTypeKey,
							(block) => html`
								<uui-card-block-type
									name=${ifDefined(block.label)}
									background=${ifDefined(block.backgroundColor)}
									style="color: ${block.iconColor}">
									<uui-icon .name=${block.icon ?? ''}></uui-icon>
								</uui-card-block-type>
							`,
						)}
					</div>
				`,
			)}
		`;
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
			uui-box .blockGroup {
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
