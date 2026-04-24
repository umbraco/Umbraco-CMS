import type { UmbTreeItemModel } from '../types.js';
import type { ManifestTreeItemCard } from './tree-item-card.extension.js';
import { UmbDefaultTreeItemCardElement } from './default/default-tree-item-card.element.js';
import { UmbDefaultTreeItemCardApi } from './default/default-tree-item-card.api.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree-item-card')
export class UmbTreeItemCardElement extends UmbLitElement {
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	#extensionsController?: any;
	#item?: UmbTreeItemModel;
	#fallbackApi?: UmbDefaultTreeItemCardApi;

	@state()
	protected _component?: any;

	@property({ type: Object, attribute: false })
	public set item(value: UmbTreeItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (value === oldValue) return;
		if (!value) return;

		if (this._component && value.entityType === oldValue?.entityType) {
			this._component.item = value;
			this.#fallbackApi?.setTreeItem(value);
			return;
		}

		this.#createController(value.entityType);
	}
	public get item(): UmbTreeItemModel | undefined {
		return this.#item;
	}

	#createController(entityType: string) {
		this.#extensionsController?.destroy();
		this.#fallbackApi?.destroy();
		this.#fallbackApi = undefined;

		this.#extensionsController = new UmbExtensionsElementAndApiInitializer(
			this,
			umbExtensionsRegistry,
			'treeItemCard',
			undefined,
			(manifest: ManifestTreeItemCard) => manifest.forEntityTypes.includes(entityType),
			(extensionControllers) => {
				if (this._component) {
					this._component.remove();
				}

				const ctrl = extensionControllers[0];

				const component = ctrl?.component ?? new UmbDefaultTreeItemCardElement();
				const api: UmbDefaultTreeItemCardApi | undefined = ctrl?.api ?? (() => {
					const fallback = new UmbDefaultTreeItemCardApi(component);
					this.#fallbackApi = fallback;
					return fallback;
				})();

				component.item = this.#item;
				component.api = api;
				api.setTreeItem(this.#item);

				this._component = component;
				this.requestUpdate('_component');
			},
			undefined,
			undefined,
			undefined,
			{ single: true },
		);
	}

	override render() {
		return html`${this._component}`;
	}

	override destroy(): void {
		this.#extensionsController?.destroy();
		this.#fallbackApi?.destroy();
		super.destroy();
	}

	static override styles = css`
		:host {
			display: block;
			position: relative;
		}
	`;
}

export default UmbTreeItemCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-card': UmbTreeItemCardElement;
	}
}
