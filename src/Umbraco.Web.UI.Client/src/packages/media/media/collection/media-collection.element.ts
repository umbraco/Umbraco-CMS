import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import type { UmbDropzoneMediaElement } from '../dropzone/index.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import type { UmbMediaCollectionOrderByOption } from './sort-preference/types.js';
import { customElement, html, ref, state, when, css, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbDropzoneSubmittedEvent } from '@umbraco-cms/backoffice/dropzone';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';
import '../components/empty-media-state/index.js';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: typeof UMB_MEDIA_COLLECTION_CONTEXT.TYPE;

	@state()
	private _progress = -1;

	@state()
	private _unique: string | null = null;

	@state()
	private _orderByOptions: Array<UmbMediaCollectionOrderByOption> = [];

	@state()
	private _activeOrderByOption?: UmbMediaCollectionOrderByOption;

	@query('#dropzone')
	private _dropzone?: UmbDropzoneMediaElement;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance?.unique, (unique) => {
				this._unique = unique ?? null;
			});
		});

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (instance) => {
			this.#collectionContext = instance;
			this.#observeOrderByOptions();
		});
	}

	#observeOrderByOptions() {
		if (!this.#collectionContext) return;

		this.observe(
			observeMultiple([this.#collectionContext.orderByOptions, this.#collectionContext.activeOrderByOption]),
			([options, activeOption]) => {
				if (this._orderByOptions.length !== options.length) {
					this._orderByOptions = options;
				}

				if (activeOption && activeOption !== this._activeOrderByOption?.unique) {
					this._activeOrderByOption = this._orderByOptions.find((option) => option.unique === activeOption);
				}
			},
			'_umbObserveMediaOrderByOptions',
		);
	}

	#observeProgressItems(dropzone?: Element) {
		if (!dropzone) return;
		this.observe(
			(dropzone as UmbDropzoneMediaElement).progressItems(),
			(progressItems) => {
				progressItems.forEach((item) => {
					if (item.folder?.name) return;

					this.#collectionContext?.updatePlaceholderStatus(item.unique, item.status);
					this.#collectionContext?.updatePlaceholderProgress(item.unique, item.progress);
				});
			},
			'_observeProgressItems',
		);
	}

	async #setupPlaceholders(event: UmbDropzoneSubmittedEvent) {
		event.preventDefault();
		const placeholders = event.items
			.filter((p) => p.parentUnique === this._unique)
			.map((p) => ({ unique: p.unique, status: p.status, name: p.temporaryFile?.file.name ?? p.folder?.name }));

		this.#collectionContext?.setPlaceholders(placeholders);
	}

	async #onComplete(event: Event) {
		event.preventDefault();
		this._progress = -1;

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Could not get event context');
		}
		const reloadEvent = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: this._unique ? UMB_MEDIA_ENTITY_TYPE : UMB_MEDIA_ROOT_ENTITY_TYPE,
			unique: this._unique,
		});
		eventContext.dispatchEvent(reloadEvent);
	}

	#onProgress(event: ProgressEvent) {
		event.preventDefault();
		this._progress = (event.loaded / event.total) * 100;
		if (this._progress >= 100) {
			this._progress = -1;
		}
	}

	#onOrderByChange(option: UmbMediaCollectionOrderByOption) {
		this.#collectionContext?.setActiveOrderByOption(option.unique);
	}

	#getOrderByIcon() {
		if (!this._activeOrderByOption) return 'icon-sort';

		return this._activeOrderByOption.config.orderDirection === 'desc' ? 'icon-arrow-down' : 'icon-arrow-up';
	}

	#getOrderByButtonLabel() {
		const orderBy = this.localize.term('general_orderBy');
		const activeOption = this._activeOrderByOption
			? this.localize.string(this._activeOrderByOption.label)
			: undefined;

		return activeOption ? `${orderBy}: ${activeOption}` : orderBy;
	}

	#renderOrderBy() {
		return html`
			<uui-button
				popovertarget="popover-media-order-by"
				label=${this.#getOrderByButtonLabel()}
				look="outline"
				compact>
				<uui-icon name=${this.#getOrderByIcon()}></uui-icon>
			</uui-button>
			<uui-popover-container id="popover-media-order-by" placement="bottom">
				<umb-popover-layout>
					<div class="filter-dropdown">
						${this._orderByOptions.map(
							(option) => html`
								<uui-menu-item
									label=${this.localize.string(option.label)}
									@click-label=${() => this.#onOrderByChange(option)}
									?active=${this._activeOrderByOption?.unique === option.unique}></uui-menu-item>
							`,
						)}
					</div>
				</umb-popover-layout>
			</uui-popover-container>
		`;
	}

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<div id="toolbar">
					<umb-collection-filter-field></umb-collection-filter-field>
					${this.#renderOrderBy()}
				</div>
			</umb-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone-media
				id="dropzone"
				${ref(this.#observeProgressItems)}
				multiple
				.parentUnique=${this._unique}
				@submitted=${this.#setupPlaceholders}
				@complete=${this.#onComplete}
				@progress=${this.#onProgress}>
			</umb-dropzone-media>
		`;
	}

	protected override _renderEmptyState() {
		return html`<umb-empty-media-state @browse=${() => this._dropzone?.browse()}> </umb-empty-media-state> `;
	}

	static override styles = [
		...UmbCollectionDefaultElement.styles,
		css`
			#toolbar {
				flex: 1;
				display: flex;
				gap: var(--uui-size-space-4);
				justify-content: space-between;
				align-items: center;
			}

			umb-collection-filter-field {
				width: 100%;
			}

			.filter-dropdown {
				display: flex;
				gap: var(--uui-size-space-3);
				flex-direction: column;
				padding: var(--uui-size-space-3);
			}

			umb-dropzone-media {
				top: var(--uui-size-layout-4);
				left: 0;
				right: 0;
				bottom: 0;
			}

			umb-empty-media-state {
				position: absolute;
				top: var(--uui-size-layout-4);
				left: var(--uui-size-layout-1);
				right: var(--uui-size-layout-1);
				bottom: calc(var(--uui-size-60) * 2);
			}
		`,
	];
}

export default UmbMediaCollectionElement;

export { UmbMediaCollectionElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-media-collection': UmbMediaCollectionElement;
	}
}
