import type { UmbTreeItemModel } from '../types.js';
import type { ManifestTreeItemCard } from './tree-item-card.extension.js';
import type { UmbTreeItemCardApi, UmbTreeItemCardElement } from './types.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementAndApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestBase } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-tree-item-card-extension')
export class UmbTreeItemCardExtensionElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementAndApiInitializer<ManifestBase, string, ManifestTreeItemCard>;
	#item?: UmbTreeItemModel;
	#api?: UmbTreeItemCardApi;

	@state()
	protected _component?: UmbTreeItemCardElement;

	@property({ type: Object, attribute: false })
	public set item(value: UmbTreeItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (value === oldValue) return;
		if (!value) return;

		if (this._component && value.entityType === oldValue?.entityType) {
			this._component.item = value;
			this.#api?.setTreeItem(value);
			return;
		}

		this.#createController(value.entityType);
	}
	public get item(): UmbTreeItemModel | undefined {
		return this.#item;
	}

	#createController(entityType: string) {
		this.#extensionsController?.destroy();

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
				if (!ctrl?.component || !ctrl?.api) return;

				const component = ctrl.component;
				const api = ctrl.api;

				component.item = this.#item;
				component.api = api;
				api.setTreeItem(this.#item);

				this.#api = api;
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
		super.destroy();
	}

	static override styles = css`
		:host {
			display: block;
			position: relative;
		}
	`;
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-card-extension': UmbTreeItemCardExtensionElement;
	}
}
