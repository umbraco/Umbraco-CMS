import type { UmbCollectionConfiguration, UmbCollectionContext } from './types.js';
import type { ManifestCollection } from './extensions/types.js';
import type { UmbCollectionFilterModel } from './collection-filter-model.interface.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';
import { UmbElementInteractionMemoryBridgeController } from '@umbraco-cms/backoffice/interaction-memory';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

@customElement('umb-collection')
export class UmbCollectionElement<
	ConfigType extends UmbCollectionConfiguration = UmbCollectionConfiguration,
	FilterType extends UmbCollectionFilterModel = UmbCollectionFilterModel,
> extends UmbExtensionElementAndApiSlotElementBase<ManifestCollection> {
	getExtensionType() {
		return 'collection';
	}

	getDefaultElementName() {
		return 'umb-collection-default';
	}

	@property({ type: Object, attribute: false })
	set config(newVal: ConfigType | undefined) {
		this.#config = newVal;
		this.#setConfig();
	}
	get config() {
		return this.#config;
	}
	#config?: ConfigType;

	@property({ type: Object, attribute: false })
	set filter(newVal: FilterType | undefined) {
		this.#filter = newVal;
		this.#setFilter();
	}
	get filter() {
		return this.#filter;
	}
	#filter?: FilterType;

	/**
	 * The memories to restore the collection with, ex. the view, filter, ordering and page it was left in.
	 */
	@property({ attribute: false })
	set interactionMemories(value: Array<UmbInteractionMemoryModel> | undefined) {
		this.#interactionMemories = value;
		this.#setInteractionMemories();
	}
	get interactionMemories() {
		return this.#interactionMemories;
	}
	#interactionMemories?: Array<UmbInteractionMemoryModel>;

	#interactionMemoryBridge?: UmbElementInteractionMemoryBridgeController;

	protected override apiChanged(api: UmbApi | undefined): void {
		super.apiChanged(api);
		this.#createInteractionMemoryBridge();
		// The memories are set before the configuration, so the collection can be configured with the memorized state.
		this.#setInteractionMemories();
		this.#setConfig();
		this.#setFilter();
	}

	#setConfig() {
		if (!this.#config || !this._api) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._api.setConfig(this.#config);
	}

	#setFilter() {
		if (!this.#filter || !this._api) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._api.setFilter(this.#filter);
	}

	#createInteractionMemoryBridge() {
		this.#interactionMemoryBridge?.destroy();
		this.#interactionMemoryBridge = undefined;

		const memoryManager = (this._api as UmbCollectionContext | undefined)?.interactionMemory;
		if (!memoryManager) return;

		this.#interactionMemoryBridge = new UmbElementInteractionMemoryBridgeController(this, memoryManager);
	}

	#setInteractionMemories() {
		if (!this.#interactionMemories) return;
		this.#interactionMemoryBridge?.setMemories(this.#interactionMemories);
	}

	/**
	 * Returns the current memories of the collection, ex. the view, filter, ordering and page it is in.
	 * @returns {Array<UmbInteractionMemoryModel>} The current memories of the collection.
	 */
	getInteractionMemories(): Array<UmbInteractionMemoryModel> {
		return this.#interactionMemoryBridge?.getMemories() ?? [];
	}

	getSelection() {
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		// TODO: make base interface for a collection menu element
		return this._element?.getSelection?.() ?? [];
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
