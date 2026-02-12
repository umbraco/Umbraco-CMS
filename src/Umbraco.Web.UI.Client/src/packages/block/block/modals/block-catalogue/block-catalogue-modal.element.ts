import { UMB_BLOCK_WORKSPACE_MODAL } from '../../workspace/index.js';
import { UMB_BLOCK_MANAGER_CONTEXT } from '../../context/index.js';
import type { UmbBlockCatalogueModalData, UmbBlockCatalogueModalValue } from './block-catalogue-modal.token.js';
import {
	css,
	customElement,
	html,
	ifDefined,
	nothing,
	repeat,
	state,
	when,
} from '@umbraco-cms/backoffice/external/lit';
import { transformServerPathToClientPath } from '@umbraco-cms/backoffice/utils';
import { UmbModalRouteRegistrationController } from '@umbraco-cms/backoffice/router';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS } from '@umbraco-cms/backoffice/document-type';
import { UMB_MODAL_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbBlockTypeGroup, UmbBlockTypeWithGroupKey } from '@umbraco-cms/backoffice/block-type';
import type { UmbDocumentTypeItemModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

type UmbBlockTypeItemWithGroupKey = UmbBlockTypeWithGroupKey & UmbDocumentTypeItemModel;

@customElement('umb-block-catalogue-modal')
export class UmbBlockCatalogueModalElement extends UmbModalBaseElement<
	UmbBlockCatalogueModalData,
	UmbBlockCatalogueModalValue
> {
	readonly #itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeItemModel>(
		this,
		UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	);

	#search = '';

	#serverUrl = '';

	private _groupedBlocks: Array<{ name?: string; blocks: Array<UmbBlockTypeItemWithGroupKey> }> = [];

	@state()
	private _openClipboard?: boolean;

	@state()
	private _workspacePath?: string;

	@state()
	private _filtered: Array<{ name?: string; blocks: Array<UmbBlockTypeItemWithGroupKey> }> = [];

	@state()
	private _manager?: typeof UMB_BLOCK_MANAGER_CONTEXT.TYPE;

	@state()
	private _loading = true;

	constructor() {
		super();

		this.consumeContext(UMB_SERVER_CONTEXT, (instance) => {
			this.#serverUrl = instance?.getServerUrl() ?? '';
		});

		this.consumeContext(UMB_MODAL_CONTEXT, (modalContext) => {
			if (modalContext?.data.createBlockInWorkspace) {
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

		this.observe(this.#itemManager.items, async (items) => {
			this.#observeBlockTypes(items);
		});
	}

	override connectedCallback() {
		super.connectedCallback();
		if (!this.data) return;

		this._openClipboard = this.data.openClipboard ?? false;

		this.#itemManager.setUniques(this.data.blocks.map((block) => block.contentElementTypeKey));
	}

	#observeBlockTypes(items: Array<UmbDocumentTypeItemModel> | undefined) {
		if (!items?.length) return;

		const lookup = items.reduce(
			(acc, item) => {
				acc[item.unique] = {
					...item,
					name: this.localize.string(item.name),
					description: this.localize.string(item.description),
				};
				return acc;
			},
			{} as { [key: string]: UmbDocumentTypeItemModel },
		);

		const blocks: Array<UmbBlockTypeItemWithGroupKey> =
			this.data?.blocks?.map((block) => ({ ...(lookup[block.contentElementTypeKey] ?? {}), ...block })) ?? [];

		const blockGroups: Array<UmbBlockTypeGroup> = this.data?.blockGroups ?? [];

		const noGroupBlocks = blocks.filter((block) => !blockGroups.find((group) => group.key === block.groupKey));

		const grouped = blockGroups.map((group) => ({
			name: this.localize.string(group.name),
			blocks: blocks.filter((block) => block.groupKey === group.key),
		}));

		this._groupedBlocks = [{ blocks: noGroupBlocks }, ...grouped];

		this.#updateFiltered();

		this._loading = false;
	}

	#updateFiltered() {
		if (this.#search.length === 0) {
			this._filtered = this._groupedBlocks;
		} else {
			const search = this.#search.toLowerCase();
			this._filtered = this._groupedBlocks.map((group) => {
				return {
					...group,
					blocks: group.blocks.filter(
						(block) =>
							block.label?.toLowerCase().includes(search) ||
							block.name?.toLowerCase().includes(search) ||
							block.description?.toLowerCase().includes(search),
					),
				};
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
			<umb-body-layout headline=${this.localize.term('blockEditor_addBlock')}>
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
		return html`
			<umb-clipboard-entry-picker
				.config=${{ multiple: true, asyncFilter: this.data?.clipboardFilter }}
				@selection-change=${this.#onClipboardPickerSelectionChange}></umb-clipboard-entry-picker>
		`;
	}

	#renderCreateEmpty() {
		if (this._loading) return html`<div id="loader"><uui-loader></uui-loader></div>`;
		return html`
			${when(
				this.data?.blocks && this.data?.blocks.length > 8,
				() => html`
					<uui-input
						id="search"
						@input=${this.#onSearch}
						label=${this.localize.term('general_search')}
						placeholder=${this.localize.term('placeholders_search')}>
						<uui-icon name="icon-search" slot="prepend"></uui-icon>
					</uui-input>
				`,
			)}
			${repeat(
				this._filtered,
				(group) => group.name,
				(group) => html`
					${when(group.name && group.blocks.length !== 0 && group.name !== '', () => html`<h4>${group.name}</h4>`)}
					<div class="blockGroup">
						${repeat(
							group.blocks,
							(block) => block.contentElementTypeKey,
							(block) => this.#renderBlockTypeCard(block),
						)}
					</div>
				`,
			)}
		`;
	}

	#renderBlockTypeCard(block: UmbBlockTypeItemWithGroupKey) {
		const href =
			this._workspacePath && this._manager!.getContentTypeHasProperties(block.contentElementTypeKey)
				? `${this._workspacePath}create/${block.contentElementTypeKey}`
				: undefined;

		const path = block.thumbnail ? transformServerPathToClientPath(block.thumbnail) : undefined;
		const imgSrc = path ? new URL(path, this.#serverUrl)?.href : undefined;

		return html`
			<uui-card-block-type
				href=${ifDefined(href)}
				name=${block.name}
				description=${ifDefined(block.description)}
				.background=${block.backgroundColor}
				@open=${() => this.#chooseBlock(block.contentElementTypeKey)}>
				${when(
					imgSrc,
					(src) => html`<img src=${src} alt="" />`,
					() => html`<umb-icon name=${block.icon ?? ''} color=${ifDefined(block.iconColor)}></umb-icon>`,
				)}
				<slot name="actions" slot="actions"> </slot>
			</uui-card-block-type>
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
			#loader {
				display: flex;
				justify-content: center;
			}

			#search {
				width: 100%;
				align-items: center;
				margin-bottom: var(--uui-size-layout-1);

				> uui-icon {
					padding-left: var(--uui-size-space-3);
				}
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
