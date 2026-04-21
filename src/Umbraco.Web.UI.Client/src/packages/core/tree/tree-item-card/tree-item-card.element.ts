import type { ManifestTreeItemCard } from './tree-item-card.extension.js';
import type { UmbTreeItemModel } from '../types.js';
import { UmbDefaultTreeItemCardElement } from './default/default-tree-item-card.element.js';
import { UmbTreeItemOpenEvent } from '../tree-item/events/tree-item-open.event.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';

@customElement('umb-tree-item-card')
export class UmbTreeItemCardElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementInitializer<any>;
	#item?: UmbTreeItemModel;

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
			return;
		}

		this.#createController(value.entityType);
	}
	public get item(): UmbTreeItemModel | undefined {
		return this.#item;
	}

	#selectable = false;
	@property({ type: Boolean, reflect: true })
	public get selectable() {
		return this.#selectable;
	}
	public set selectable(value) {
		this.#selectable = value;
		if (this._component) this._component.selectable = value;
	}

	#selected = false;
	@property({ type: Boolean, reflect: true })
	public get selected() {
		return this.#selected;
	}
	public set selected(value) {
		this.#selected = value;
		if (this._component) this._component.selected = value;
	}

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		if (!this.#item) return;
		this.dispatchEvent(new UmbSelectedEvent(this.#item.unique));
	}

	#onDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		if (!this.#item) return;
		this.dispatchEvent(new UmbDeselectedEvent(this.#item.unique));
	}

	#onOpen(event: UmbTreeItemOpenEvent) {
		event.stopPropagation();
		this.dispatchEvent(new UmbTreeItemOpenEvent({ unique: event.unique, entityType: event.entityType }));
	}

	#boundOnSelected = this.#onSelected.bind(this);
	#boundOnDeselected = this.#onDeselected.bind(this);
	#boundOnOpen = this.#onOpen.bind(this);

	#createController(entityType: string) {
		this.#extensionsController?.destroy();

		this.#extensionsController = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'treeItemCard',
			(manifest: ManifestTreeItemCard) => manifest.forEntityTypes.includes(entityType),
			(extensionControllers) => {
				if (this._component) {
					this._component.removeEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
					this._component.removeEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);
					this._component.removeEventListener(UmbTreeItemOpenEvent.TYPE, this.#boundOnOpen);
					this._component.remove();
				}

				const component = extensionControllers[0]?.component ?? new UmbDefaultTreeItemCardElement();

				component.item = this.#item;
				component.selectable = this.#selectable;
				component.selected = this.#selected;

				component.addEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
				component.addEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);
				component.addEventListener(UmbTreeItemOpenEvent.TYPE, this.#boundOnOpen);

				this._component = component;
				this.requestUpdate('_component');
			},
			undefined,
			undefined,
			{ single: true },
		);
	}

	override render() {
		return html`${this._component}`;
	}

	override destroy(): void {
		this._component?.removeEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
		this._component?.removeEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);
		this._component?.removeEventListener(UmbTreeItemOpenEvent.TYPE, this.#boundOnOpen);
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

export default UmbTreeItemCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-tree-item-card': UmbTreeItemCardElement;
	}
}
