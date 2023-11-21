import { customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { ManifestCollection, umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { createExtensionApi, createExtensionElement } from '@umbraco-cms/backoffice/extension-api';
import { UmbUserCollectionContext } from 'src/packages/user/user/collection/user-collection.context';
import { UmbCollectionContext } from './types';

@customElement('umb-collection')
export class UmbCollectionElement extends UmbLitElement {
	_alias?: string;
	@property({ type: String, reflect: true })
	get alias() {
		return this._alias;
	}
	set alias(newVal) {
		this._alias = newVal;
		this.#observeManifest();
	}

	@state()
	_element: HTMLElement | undefined;

	#manifest?: ManifestCollection;

	#observeManifest() {
		if (!this._alias) return;
		this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('collection', this._alias),
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
		const api = (await createExtensionApi(this.#manifest, [this])) as UmbCollectionContext;
		if (!api) throw new Error('No api');
		api.setManifest(this.#manifest);
	}

	async #createElement() {
		if (!this.#manifest) throw new Error('No manifest');
		this._element = await createExtensionElement(this.#manifest);
		this.requestUpdate();
	}

	render() {
		return html`${this._element}`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-collection': UmbCollectionElement;
	}
}
