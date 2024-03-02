import type { UmbCollectionConfiguration, UmbCollectionContext } from './types.js';
import { customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionApi, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestCollection } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
	#alias?: string;
	@property({ type: String, reflect: true })
	set alias(newVal) {
		this.#alias = newVal;
		this.#observeManifest();
	}
	get alias() {
		return this.#alias;
	}

	#config?: UmbCollectionConfiguration = { pageSize: 50 };
	@property({ type: Object, attribute: false })
	set config(newVal: UmbCollectionConfiguration | undefined) {
		this.#config = newVal;
		this.#setConfig();
	}
	get config() {
		return this.#config;
	}

	@state()
	_element: HTMLElement | undefined;

	#manifest?: ManifestCollection;

	#api?: UmbCollectionContext;

	#observeManifest() {
		if (!this.#alias) return;
		this.observe(
			umbExtensionsRegistry.byTypeAndAlias('collection', this.#alias),
			async (manifest) => {
				if (!manifest) return;
				this.#manifest = manifest;
				this.#createApi();
				this.#createElement();
			},
			'umbObserveCollectionManifest',
		);
	}

	async #createApi() {
		if (!this.#manifest) throw new Error('No manifest');
		this.#api = (await createExtensionApi(this.#manifest, [this])) as unknown as UmbCollectionContext;
		if (!this.#api) throw new Error('No api');
		this.#api.setManifest(this.#manifest);
		this.#setConfig();
	}

	async #createElement() {
		if (!this.#manifest) throw new Error('No manifest');
		this._element = await createExtensionElement(this.#manifest);
		this.requestUpdate();
	}

	#setConfig() {
		if (!this.#config || !this.#api) return;
		this.#api.setConfig(this.#config);
	}

	render() {
		return this._element;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
