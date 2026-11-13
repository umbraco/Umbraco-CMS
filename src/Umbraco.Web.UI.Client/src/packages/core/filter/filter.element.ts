import { UmbFilterContext } from './filter.context.js';
import type { ManifestFilterBase } from './filter.extension.js';
import type { UmbFilterExtensionElement } from './filter-extension-element.interface.js';
import type { UmbFilterExtensionApi } from './filter-extension-api.interface.js';
import { css, customElement, html, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { createExtensionElementWithApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';

/**
 * Renders one filter: the element and API of its filter extension, under a header with the filter's label and a
 * button that clears its values. It provides the filter context the extension reads and writes those values through.
 * @element umb-filter
 */
@customElement('umb-filter')
export class UmbFilterElement extends UmbLitElement {
	#context = new UmbFilterContext(this);

	#manifest?: ManifestFilterBase;
	/**
	 * The manifest of the filter extension to render, merged with its kind. The host that decides which filters apply
	 * passes it in, so the filter is not looked up again. Conditions are not evaluated here: pass only manifests whose
	 * conditions are met, for example the permitted manifests of an `UmbExtensionsManifestInitializer`.
	 * @returns {ManifestFilterBase | undefined} The filter manifest.
	 */
	@property({ attribute: false })
	public get manifest(): ManifestFilterBase | undefined {
		return this.#manifest;
	}
	public set manifest(manifest: ManifestFilterBase | undefined) {
		if (manifest === this.#manifest) return;
		const oldValue = this.#manifest;
		this.#manifest = manifest;
		if (manifest) {
			this.#context.setAlias(manifest.alias);
		}
		this.#createExtension(manifest);
		this.requestUpdate('manifest', oldValue);
	}

	@state()
	private _extensionElement?: UmbFilterExtensionElement;

	#extensionApi?: UmbFilterExtensionApi;

	@state()
	private _hasValues = false;

	constructor() {
		super();
		this.observe(this.#context.values, (values) => (this._hasValues = values.length > 0), null);
	}

	async #createExtension(manifest: ManifestFilterBase | undefined) {
		this.#destroyExtension();
		if (!manifest) return;

		const { element, api } = await createExtensionElementWithApi<UmbFilterExtensionElement, UmbFilterExtensionApi>(
			manifest,
		);

		// The manifest may have changed while the extension was loading.
		if (manifest !== this.#manifest) {
			api?.destroy();
			return;
		}

		if (api) {
			api.manifest = manifest;
		}
		if (element) {
			element.manifest = manifest;
			element.api = api;
		}
		this.#extensionApi = api;
		this._extensionElement = element;
	}

	#destroyExtension() {
		this.#extensionApi?.destroy();
		this.#extensionApi = undefined;
		this._extensionElement = undefined;
	}

	#onClear() {
		this.#context.clearAllValues();
	}

	override destroy() {
		this.#destroyExtension();
		super.destroy();
	}

	override render() {
		return html`
			<div id="header">
				<span id="label">${this.localize.string(this.#manifest?.meta.label ?? '')}</span>
				<uui-button
					compact
					look="secondary"
					label=${this.localize.term('general_clear')}
					?disabled=${!this._hasValues}
					@click=${this.#onClear}></uui-button>
			</div>
			${this._extensionElement ?? nothing}
		`;
	}

	static override styles = [
		css`
			:host {
				display: flex;
				flex-direction: column;
				gap: var(--uui-size-space-2);
			}

			#header {
				display: flex;
				justify-content: space-between;
				align-items: center;
				min-height: var(--uui-size-8);
			}

			#label {
				font-weight: 700;
				font-size: var(--uui-type-default-size);
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-filter': UmbFilterElement;
	}
}
