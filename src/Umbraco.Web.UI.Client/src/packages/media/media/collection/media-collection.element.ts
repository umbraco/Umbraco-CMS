import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_WORKSPACE_CONTEXT } from '../workspace/media-workspace.context-token.js';
import type { UmbDropzoneMediaElement } from '../dropzone/index.js';
import { UMB_MEDIA_COLLECTION_CONTEXT } from './media-collection.context-token.js';
import { customElement, html, ref, state, when, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
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

	constructor() {
		super();

		this.consumeContext(UMB_MEDIA_COLLECTION_CONTEXT, (context) => {
			// TODO: stop consuming the context both in the default element and here. Instead make the default able to inform when the context is consumed. Or come up with a better system for the controllers to talk together. [NL]
			this.#collectionContext = context;
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

	protected override renderToolbar() {
		return html`
			<umb-collection-toolbar slot="header">
				<umb-collection-filter-field></umb-collection-filter-field>
			</umb-collection-toolbar>
			${when(this._progress >= 0, () => html`<uui-loader-bar progress=${this._progress}></uui-loader-bar>`)}
			<umb-dropzone-media
				id="dropzone"
				${ref(this.#observeProgressItems)}
				multiple
				.parentUnique=${this._unique}
				@submitted=${this.#setupPlaceholders}
				@complete=${this.#onComplete}
				@progress=${this.#onProgress}></umb-dropzone-media>
		`;
	}

	protected override renderEmptyState() {
		return html`
			<div class="media-empty-state">
				
				<span class="empty-state-title">
					<umb-localize key="media_dragAndDropYourFilesIntoTheArea">
						Drag and drop your file(s) into the area
					</umb-localize>
				</span>

				<uui-icon name="icon-cloud-upload" class="empty-state-icon"></uui-icon>

				<button 
					type="button" 
					class="empty-state-browse-btn"
					@click=${() => this.#triggerFileBrowser()}>
					<umb-localize key="media_orClickHereToChooseFiles">
						- or click here to choose files
					</umb-localize>
				</button>

			</div>
		`;
	}

	#triggerFileBrowser() {
		const dropzoneWrapper = this.shadowRoot?.querySelector('#dropzone');
		const uuiDropzone = dropzoneWrapper?.shadowRoot?.querySelector('uui-file-dropzone');
		const nativeInput = uuiDropzone?.shadowRoot?.querySelector('input[type="file"]') as HTMLInputElement;

		if (nativeInput) nativeInput.click();
	}

	static override styles = [
		UmbTextStyles,
		css`
			.media-empty-state {
				display: flex;
				flex-direction: column;
				align-items: center;
				justify-content: center;
				margin-top: 15vh; 
				width: 100%;
				gap: var(--uui-size-space-4);
			}

			.empty-state-title {
				color: var(--uui-color-text-alt);
				font-size: 1rem;
			}

			.empty-state-icon {
				font-size: 7rem; 
				color: var(--uui-color-border-standalone);
				opacity: 0.3; 
			}

			.empty-state-browse-btn {
				background: none;
				border: none;
				padding: 0;
				color: var(--uui-color-interactive);
				font-size: 1rem; 
				cursor: pointer;
				text-decoration: none;
			}

			.empty-state-browse-btn:hover {
				text-decoration: underline;
				color: var(--uui-color-interactive-emphasis);
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
