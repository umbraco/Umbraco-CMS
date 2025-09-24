import { UMB_BLOCK_WORKSPACE_MODAL } from '../../workspace/index.js';
import { UMB_BLOCK_MANAGER_CONTEXT } from '../../context/index.js';
import type { UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue } from './block-catalogue-modal.token.js';
import type { UmbBlockTypeGroup, UmbBlockTypeWithGroupKey } from '@umbraco-cms/backoffice/block-type';
import { css, html, customElement, state, repeat, nothing } from '@umbraco-cms/backoffice/external/lit';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';
import { UMB_MODAL_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';

// TODO: This is across packages, how should we go about getting just a single element from another package? like here we just need the umb-block-type-card element
import '@umbraco-cms/backoffice/block-type';

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	#search = '';

	private _groupedBlocks: Array<{ name?: string; blocks: Array<UmbBlockTypeWithGroupKey> }> = [];

	@state()
	private _openClipboard?: boolean;

	@state()
	private _workspacePath?: string;

	@state()
	private _filtered: Array<{ name?: string; blocks: Array<UmbBlockTypeWithGroupKey> }> = [];

	@state()
	_manager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;

	constructor() {
		super();

		this.consumeContext(UMB_MODAL_CONTEXT, (modalContext) => {
			if (modalContext.data.createBlockInWorkspace) {
				new UmbModalRouteRegistrationController(this, UMB_BLOCK_WORKSPACE_MODAL)
					//.addAdditionalPath('block') // No need for additional path specification in this context as this is for sure the only workspace we want to open here.
					.onSetup(() => {
						return {
							data: { preset: {}, originData: (modalContext.data as UmbBlockCatalogueModalData).originData },
						};
					})
					.onSubmit(() => {
						// When workspace is submitted, we want to close this modal.
						this.modalContext?.submit();
					})
					.observeRouteBuilder((routeBuilder) => {
						this._workspacePath = routeBuilder({});
					});
			}
		});

		this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (manager) => {
			this._manager = manager;
		});
	}

	override connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		this._openClipboard = this.data.openClipboard ?? false;

		const blocks: Array<UmbBlockTypeWithGroupKey> = this.data.blocks ?? [];
		const blockGroups: Array<UmbBlockTypeGroup> = this.data.blockGroups ?? [];

		const noGroupBlocks = blocks.filter((block) => !blockGroups.find((group) => group.key === block.groupKey));
		const grouped = blockGroups.map((group) => ({
			name: group.name,
			blocks: blocks.filter((block) => block.groupKey === group.key),
		}));

		this._groupedBlocks = [{ blocks: noGroupBlocks }, ...grouped];
		this.#updateFiltered();
	}

	#updateFiltered() {
		if (this.#search.length === 0) {
			this._filtered = this._groupedBlocks;
		} else {
			const search = this.#search.toLowerCase();
			this._filtered = this._groupedBlocks.map((group) => {
				return { ...group, blocks: group.blocks.filter((block) => block.label?.toLocaleLowerCase().includes(search)) };
			});
		}
	}

	#onSearch(e: UUIInputEvent) {
		this.#search = e.target.value as string;
		this.#updateFiltered();
	}

	#chooseBlock(contentElementTypeKey: string) {
		this.value = {
			create: {
				contentElementTypeKey,
			},
		};
		this.modalContext?.submit();
	}

	async #onClipboardPickerSelectionChange(event: UmbSelectionChangeEvent) {
		const target = event.target as any;
		const selection = target?.selection || [];
		this.value = {
			clipboard: {
				selection,
			},
		};
	}

	override render() {
		return html`
			<umb-body-layout headline="${this.localize.term('blockEditor_addBlock')}">
				${this.#renderViews()}${this.#renderMain()}
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

	#renderMain() {
		return this._manager ? (this._openClipboard ? this.#renderClipboard() : this.#renderCreateEmpty()) : nothing;
	}

	#renderClipboard() {
		return html`<uui-box
			><umb-clipboard-entry-picker
				.config=${{ multiple: true, asyncFilter: this.data?.clipboardFilter }}
				@selection-change=${this.#onClipboardPickerSelectionChange}></umb-clipboard-entry-picker
		></uui-box>`;
	}

	#renderCreateEmpty() {
		return html`
			${this.data?.blocks && this.data.blocks.length > 8
				? html`<uui-input
						id="search"
						@input=${this.#onSearch}
						label=${this.localize.term('general_search')}
						placeholder=${this.localize.term('placeholders_search')}>
						<uui-icon name="icon-search" slot="prepend"></uui-icon>
					</uui-input>`
				: nothing}
			${this._filtered.map(
				(group) => html`
					${group.name && group.blocks.length !== 0 && group.name !== '' ? html`<h4>${group.name}</h4>` : nothing}
					<div class="blockGroup">
						${repeat(
							group.blocks,
							(block) => block.contentElementTypeKey,
							(block) => html`
								<umb-block-type-card
									.iconFile=${block.thumbnail}
									.iconColor=${block.iconColor}
									.backgroundColor=${block.backgroundColor}
									.contentElementTypeKey=${block.contentElementTypeKey}
									@open=${() => this.#chooseBlock(block.contentElementTypeKey)}
									.href=${this._workspacePath && this._manager!.getContentTypeHasProperties(block.contentElementTypeKey)
										? `${this._workspacePath}create/${block.contentElementTypeKey}`
										: undefined}>
								</umb-block-type-card>
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
					<uui-icon slot="icon" name="icon-clipboard"></uui-icon>
				</uui-tab>
			</uui-tab-group>
		`;
	}

	static override styles = [
		css`
			#search {
				width: 100%;
				align-items: center;
				margin-bottom: var(--uui-size-layout-1);
			}
			#search uui-icon {
				padding-left: var(--uui-size-space-3);
			}
			.blockGroup {
				display: grid;
				gap: 1rem;
				grid-template-columns: repeat(auto-fill, minmax(min(var(--umb-card-medium-min-width), 100%), 1fr));
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
