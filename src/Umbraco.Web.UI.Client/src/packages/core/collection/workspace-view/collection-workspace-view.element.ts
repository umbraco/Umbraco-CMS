import type { UmbCollectionConfiguration } from '../types.js';
import type { UmbCollectionElement } from '../collection.element.js';
import { UmbCollectionWorkspaceViewInteractionMemoryController } from '../interaction-memory/index.js';
import type { ManifestWorkspaceViewCollectionKind } from './types.js';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';
import type { PropertyValues } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-collection-workspace-view')
export class UmbCollectionWorkspaceViewElement extends UmbLitElement {
	@property({ type: Object, attribute: false })
	public manifest?: ManifestWorkspaceViewCollectionKind;

	@state()
	protected _config?: UmbCollectionConfiguration;

	@state()
	protected _filter?: unknown;

	@state()
	private _collectionInteractionMemories?: Array<UmbInteractionMemoryModel>;

	#workspaceViewInteractionMemory = new UmbCollectionWorkspaceViewInteractionMemoryController(this);

	constructor() {
		super();

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
			this.#workspaceViewInteractionMemory.setCollectionAlias(this.manifest?.meta.collectionAlias);
		}
	}

	#onCollectionInteractionMemoriesChange(event: Event) {
		event.stopPropagation();
		const collection = event.currentTarget as UmbCollectionElement;
		this.#workspaceViewInteractionMemory.writeInteractionMemory(collection.getInteractionMemories());
	}

	override render() {
		if (!this.manifest) return html` <div>No Manifest</div>`;
		if (!this.manifest.meta.collectionAlias) return html` <div>No Collection Alias in Manifest</div>`;
		return html`<umb-collection
			data-mark="collection:${this.manifest.meta.collectionAlias}"
			alias=${this.manifest.meta.collectionAlias}
			.config=${this._config}
			.filter=${this._filter}
			.interactionMemories=${this._collectionInteractionMemories}
			@interaction-memories-change=${this.#onCollectionInteractionMemoriesChange}></umb-collection>`;
	}
}

export { UmbCollectionWorkspaceViewElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection-workspace-view': UmbCollectionWorkspaceViewElement;
	}
}
