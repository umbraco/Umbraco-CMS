import { UMB_BLOCK_WORKSPACE_MODAL } from '../../workspace/index.js';
import type {
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue,
	UmbBlockTypeGroup,
	UmbBlockTypeWithGroupKey,
} from '@umbraco-cms/backoffice/block';
import { css, html, customElement, state, repeat, ifDefined, nothing } from '@umbraco-cms/backoffice/external/lit';
import {
	UMB_MODAL_CONTEXT,
	UmbModalBaseElement,
	UmbModalRouteRegistrationController,
} from '@umbraco-cms/backoffice/modal';

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	@state()
	private _groupedBlocks: Array<{ name?: string; blocks: Array<UmbBlockTypeWithGroupKey> }> = [];

	@state()
	_openClipboard?: boolean;

	@state()
	_workspacePath?: string;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT, (modalContext) => {
			new UmbModalRouteRegistrationController(this, UMB_BLOCK_WORKSPACE_MODAL)
				//.addAdditionalPath('block') // No need for additional path specification in this context as this is for sure the only workspace we want to open here.
				.onSetup(() => {
					return {
						data: { preset: {}, originData: (modalContext.data as UmbBlockCatalogueModalData).blockOriginData },
					};
				})
				.onSubmit(() => {
					// When workspace is submitted, we want to close this modal.
					this.modalContext?.submit();
				})
				.observeRouteBuilder((routeBuilder) => {
					this._workspacePath = routeBuilder({});
				});
		});
	}

	connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		this._openClipboard = this.data.openClipboard ?? false;

		const blocks: Array<UmbBlockTypeWithGroupKey> = this.data.blocks ?? [];
		const blockGroups: Array<UmbBlockTypeGroup> = this.data.blockGroups ?? [];

		const noGroupBlocks = blocks.filter((block) => !blockGroups.find((group) => group.key === block.groupKey));
		const grouped = blockGroups.map((group) => ({
			name: group.name ?? '',
			blocks: blocks.filter((block) => block.groupKey === group.key),
		}));

		this._groupedBlocks = [{ blocks: noGroupBlocks }, ...grouped];
	}

	render() {
		return html`
			<umb-body-layout headline="${this.localize.term('blockEditor_addBlock')}">
				${this.#renderViews()} ${this._openClipboard ? this.#renderClipboard() : this.#renderCreateEmpty()}
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
		return html`
			${this._groupedBlocks.map(
				(group) => html`
					${group.name ? html`<h4>${group.name}</h4>` : nothing}
					<div class="blockGroup">
						${repeat(
							group.blocks,
							(block) => block.contentElementTypeKey,
							(block) => html`
								<uui-card-block-type
									name=${ifDefined(block.label)}
									background=${ifDefined(block.backgroundColor)}
									style="color: ${block.iconColor}"
									href="${this._workspacePath}create/${block.contentElementTypeKey}">
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
					label=${this.localize.term('blockEditor_tabCreateEmpty')}
					?active=${!this._openClipboard}
					@click=${() => (this._openClipboard = false)}>
					<umb-localize key=${this.localize.term('blockEditor_tabCreateEmpty')}>Create Empty</umb-localize>
					<uui-icon slot="icon" name="icon-add"></uui-icon>
				</uui-tab>
				<uui-tab
					label=${this.localize.term('blockEditor_tabClipboard')}
					?active=${this._openClipboard}
					@click=${() => (this._openClipboard = true)}>
					<umb-localize key=${this.localize.term('blockEditor_tabClipboard')}>Clipboard</umb-localize>
					<uui-icon slot="icon" name="icon-paste-in"></uui-icon>
				</uui-tab>
			</uui-tab-group>
		`;
	}

	static styles = [
		css`
			.blockGroup {
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
