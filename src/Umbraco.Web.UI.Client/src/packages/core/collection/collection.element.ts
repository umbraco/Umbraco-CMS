import type { UmbCollectionConfiguration } from './types.js';
import type { ManifestCollection } from './extensions/types.js';
import type { UmbCollectionFilterModel } from './collection-filter-model.interface.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

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

	protected override apiChanged(api: UmbApi | undefined): void {
		super.apiChanged(api);
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
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
