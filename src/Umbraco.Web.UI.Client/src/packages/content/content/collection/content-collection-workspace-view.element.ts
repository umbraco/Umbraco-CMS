import { UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT } from './content-collection-workspace.context-token.js';
import { customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbCollectionWorkspaceViewInteractionMemoryController } from '@umbraco-cms/backoffice/collection';
import type { UmbCollectionConfiguration, UmbCollectionElement } from '@umbraco-cms/backoffice/collection';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { ManifestWorkspaceView, UmbWorkspaceViewElement } from '@umbraco-cms/backoffice/workspace';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-content-collection-workspace-view')
export class UmbContentCollectionWorkspaceViewElement extends UmbLitElement implements UmbWorkspaceViewElement {
	@property({ type: Object, attribute: false })
	public manifest?: ManifestWorkspaceView;

	@state()
	private _loading = true;

	@state()
	private _config?: UmbCollectionConfiguration;

	@state()
	private _collectionAlias?: string;

	@state()
	private _collectionInteractionMemories?: Array<UmbInteractionMemoryModel>;

	#workspaceViewInteractionMemory = new UmbCollectionWorkspaceViewInteractionMemoryController(this);

	constructor() {
		super();

		this.consumeContext(UMB_CONTENT_COLLECTION_WORKSPACE_CONTEXT, (workspaceContext) => {
			this._collectionAlias = workspaceContext?.collection.getCollectionAlias();
			this.#workspaceViewInteractionMemory.setCollectionAlias(this._collectionAlias);

			this.observe(
				workspaceContext?.collection.collectionConfig,
				(config) => {
					if (config) {
						this._config = config;
						this._loading = false;
					}
				},
				'_observeConfigContentType',
			);
		});

		this.observe(
			this.#workspaceViewInteractionMemory.memories,
			(memories) => {
				this._collectionInteractionMemories = memories;
			},
			null,
		);
	}

	protected override updated(changedProperties: PropertyValues) {
		super.updated(changedProperties);
		if (changedProperties.has('manifest')) {
			this.#workspaceViewInteractionMemory.setWorkspaceViewAlias(this.manifest?.alias);
		}
	}

	#onCollectionInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const collection = event.currentTarget as UmbCollectionElement;
		this.#workspaceViewInteractionMemory.writeInteractionMemory(collection.getInteractionMemories());
	}

	override render() {
		if (this._loading) return nothing;
		return html`<umb-collection
			.alias=${this._collectionAlias}
			.config=${this._config}
			.interactionMemories=${this._collectionInteractionMemories}
			@interaction-memories-change=${this.#onCollectionInteractionMemoriesChange}></umb-collection>`;
	}
}

export { UmbContentCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-content-collection-workspace-view': UmbContentCollectionWorkspaceViewElement;
	}
}
