import type { UmbCollectionConfiguration, UmbCollectionContext } from './types.js';
import { customElement, property } from '@umbraco-cms/backoffice/external/lit';
import { UmbExtensionElementAndApiSlotElementBase } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbExtensionElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';

@customElement('umb-collection')
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

	#setConfig() {
		if (!this.#config || !this._api) return;
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._api.setConfig(this.#config);
	}

	protected extensionChanged = (
		isPermitted: boolean,
		controller: UmbExtensionElementAndApiInitializer<ManifestCollection>,
	) => {
		// TODO: [v15] Once `UmbCollectionContext` extends `UmbApi`, then we can remove this type casting. [LK]
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-ignore
		this._api = isPermitted ? (controller.api as unknown as UmbCollectionContext) : undefined;
		this._element = isPermitted ? controller.component : undefined;
		this.requestUpdate('_element');
		this.#setConfig();
	};
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
