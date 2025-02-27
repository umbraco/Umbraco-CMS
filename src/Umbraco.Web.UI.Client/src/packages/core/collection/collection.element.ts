import type { UmbCollectionConfiguration } from './types.js';
import type { ManifestCollection } from './extensions/types.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

const elementName = 'umb-collection';
@customElement(elementName)
export class UmbCollectionElement extends UmbExtensionElementAndApiSlotElementBase<ManifestCollection> {
	getExtensionType() {
		return 'collection';
	}

	getDefaultElementName() {
		return 'umb-collection-default';
	}

	@property({ type: Object, attribute: false })
	set config(newVal: UmbCollectionConfiguration | undefined) {
		this.#config = newVal;
		this.#setConfig();
	}
	get config() {
		return this.#config;
	}
	#config?: UmbCollectionConfiguration;

	protected override apiChanged(api: UmbApi | undefined): void {
		super.apiChanged(api);
		this.#setConfig();
	}

	#setConfig() {
		if (!this.#config || !this._api) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._api.setConfig(this.#config);
	}
}

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbCollectionElement;
	}
}
