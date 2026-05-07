import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import type { UmbDropzoneMediaElement } from '../dropzone/index.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { customElement, html, ref, state, when, css, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbCollectionDefaultElement } from '@umbraco-cms/backoffice/collection';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type { UmbDropzoneSubmittedEvent } from '@umbraco-cms/backoffice/dropzone';

@customElement('umb-media-collection')
export class UmbMediaCollectionElement extends UmbCollectionDefaultElement {
	#collectionContext?: typeof UMB_MEDIA_COLLECTION_CONTEXT.TYPE;

	@state()
	private _progress = -1;

	@state()
	private _unique: string | null = null;

	@state()
	private _isEmpty = false

	@query('#native-file-input')
	private _fileInput?: HTMLInputElement;

	@query('#dropzone')
	private _dropzone?: any;

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (context) => {
			// TODO: stop consuming the context both in the default element and here. Instead make the default able to inform when the context is consumed. Or come up with a better system for the controllers to talk together. [NL]
			this.#collectionContext = context;

			if (context) {
				this.observe(context.items, (items) => {
					this._isEmpty = items.length === 0;
				});
			}
		});

		this.consumeContext(UMB_MEDIA_WORKSPACE_CONTEXT, (instance) => {
			this.observe(instance?.unique, (unique) => {
				this._unique = unique ?? null;
			});
		});
	}

	#observeProgressItems(dropzone?: Element) {
		if (!dropzone) return;
		this.observe(
			(dropzone as UmbDropzoneMediaElement).progressItems(),
			(progressItems) => {
				progressItems.forEach((item) => {
					// We do not update folders as it may have children still being uploaded.
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

	#handleBrowseClick() {
		// Use the queried element directly
		this._fileInput?.click();
	}

	#handleFileSelect(event: Event) {
		const input = event.target as HTMLInputElement;
		if (!input.files || input.files.length === 0) return;
		
		// Use the queried element directly
		if (this._dropzone && typeof this._dropzone.onUpload === 'function') {
			this._dropzone.onUpload({ detail: { files: Array.from(input.files), folders: [] } });
		}
		
		input.value = ''; 
	}


	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone-media
				id="dropzone"
				?is-empty=${this._isEmpty} 
				${ref(this.#observeProgressItems)}
				multiple
				.parentUnique=${this._unique}
				@submitted=${this.#setupPlaceholders}
				@complete=${this.#onComplete}
				@progress=${this.#onProgress}>
			</umb-dropzone-media>
		`;
	}
	public override render() {
		return html`
			${super.render()}

			${when(
				this._isEmpty,
				() => html`
				<div class="empty-media-state">
						<uui-icon name="icon-picture"></uui-icon>
						<p>Drag and drop your media files here</p>
						<p>or</p>
						<uui-button look="primary" label="Browse files" @click=${this.#handleBrowseClick}>
							Browse files
						</uui-button>
						
						<input 
							type="file" 
							id="native-file-input" 
							multiple 
							@change=${this.#handleFileSelect} 
							style="display:none;" 
						/>
					</div>
				`
			)}
		`;
	}

	static override styles = [
		...UmbCollectionDefaultElement.styles,
		css`
			umb-dropzone-media {
				top: var(--uui-size-layout-4);
				left: 0;
				right: 0;
				bottom: 0;
			}

			.empty-media-state {
				cursor: pointer;
				position: absolute;
				top: var(--uui-size-layout-4);
				left: var(--uui-size-layout-1);
				right: var(--uui-size-layout-1);
				bottom: calc(var(--uui-size-60) * 2);
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				background-color: var(--uui-palette-sand); 
				border: 1px dashed var(--uui-color-border-standalone, --uui-palette-grey-light);
				border-radius: var(--uui-border-radius);
				color: var(--uui-color-default);
				opacity: 0;
				animation: fadeInEmptyState 0.2s ease-in forwards 0.15s; 
				pointer-events: none;
				}

			@keyframes fadeInEmptyState {
				to {
					opacity: 1;
				}
			}   
			.empty-media-state uui-icon {
				font-size: clamp(1rem, 2.5vw, 3rem);
				margin-bottom: var(--uui-size-space-4);
				color: var(--uui-color-default);
			}
			
			.empty-media-state uui-button {
				z-index: 1000; 
				position: relative;
				pointer-events: auto;
			}
			#empty-state {
				display: none;
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
