import { umbExtensionsRegistry } from './registry.js';
import { html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import type { ManifestWithDynamicConditions } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionElementInitializer, createExtensionApi } from '@umbraco-cms/backoffice/extension-api';

// TODO: Eslint: allow abstract element class to end with "ElementBase" instead of "Element"
// eslint-disable-next-line local-rules/enforce-element-suffix-on-element-class-name
export abstract class UmbExtensionInitializerElementBase<
	ManifestType extends ManifestWithDynamicConditions,
> extends UmbLitElement {
	_alias?: string;
	@property({ type: String, reflect: true })
	get alias() {
		return this._alias;
	}
	set alias(newVal) {
		this._alias = newVal;
		this.#observeManifest();
	}

	@property({ type: Object, attribute: false })
	get props() {
		return this.#props;
	}
	set props(newVal: Record<string, unknown> | undefined) {
		// TODO, compare changes since last time. only reset the ones that changed. This might be better done by the controller is self:
		this.#props = newVal;
		if (this.#extensionElementController) {
			this.#extensionElementController.properties = newVal;
		}
	}
	#props?: Record<string, unknown> = {};

	#extensionElementController?: UmbExtensionElementInitializer<ManifestType>;

	@state()
	_element: HTMLElement | undefined;

	abstract getExtensionType(): string;
	abstract getDefaultElementName(): string;

	#observeManifest() {
		if (!this._alias) return;
		this.observe(
			umbExtensionsRegistry.byTypeAndAlias(this.getExtensionType(), this._alias),
			async (m) => {
				if (!m) return;
				const manifest = m as unknown as ManifestType;
				this.createApi(manifest);
				this.createElement(manifest);
			},
			'umbObserveTreeManifest',
		);
	}

	protected async createApi(manifest?: ManifestType) {
		if (!manifest) throw new Error('No manifest');
		const api = (await createExtensionApi(manifest, [this])) as unknown as any;
		if (!api) throw new Error('No api');
		api.setManifest(manifest);
	}

	protected async createElement(manifest?: ManifestType) {
		if (!manifest) throw new Error('No manifest');

		const extController = new UmbExtensionElementInitializer<ManifestType>(
			this,
			umbExtensionsRegistry,
			manifest.alias,
			this.#extensionChanged,
			this.getDefaultElementName(),
		);

		extController.properties = this.#props;

		this.#extensionElementController = extController;
	}

	#extensionChanged = (isPermitted: boolean, controller: UmbExtensionElementInitializer<ManifestType>) => {
		this._element = isPermitted ? controller.component : undefined;
		this.requestUpdate('_element');
	};

	render() {
		return html`${this._element}`;
	}
}
