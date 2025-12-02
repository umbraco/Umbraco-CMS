import type { UmbCollectionItemDetailPropertyConfig, UmbCollectionItemModel } from './types.js';
import type { ManifestEntityCollectionItemBase } from './entity-collection-item.extension.js';
import { property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';

export abstract class UmbEntityCollectionItemElementBase extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementInitializer<any>;
	#item?: UmbCollectionItemModel;

	@state()
	protected _component?: any; // TODO: Add type

	@property({ type: Object, attribute: false })
	public set item(value: UmbCollectionItemModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (value === oldValue) return;
		if (!value) return;

		// If the component is already created and the entity type is the same, we can just update the item.
		if (this._component && value.entityType === oldValue?.entityType) {
			this._component.item = value;
			return;
		}

		this.#pathAddendum.setAddendum(this.getPathAddendum(value.entityType, value.unique));

		// If the component is already created, but the entity type is different, we need to destroy the component.
		this.#createController(value.entityType);
	}
	public get item(): UmbCollectionItemModel | undefined {
		return this.#item;
	}

	#selectable = false;
	@property({ type: Boolean, reflect: true })
	public get selectable() {
		return this.#selectable;
	}
	public set selectable(value) {
		this.#selectable = value;

		if (this._component) {
			this._component.selectable = this.#selectable;
		}
	}

	#selectOnly = false;
	@property({ type: Boolean, attribute: 'select-only', reflect: true })
	public get selectOnly() {
		return this.#selectOnly;
	}
	public set selectOnly(value) {
		this.#selectOnly = value;

		if (this._component) {
			this._component.selectOnly = this.#selectOnly;
		}
	}

	#selected = false;
	@property({ type: Boolean, reflect: true })
	public get selected() {
		return this.#selected;
	}
	public set selected(value) {
		this.#selected = value;

		if (this._component) {
			this._component.selected = this.#selected;
		}
	}

	#disabled = false;
	@property({ type: Boolean, reflect: true })
	public get disabled() {
		return this.#disabled;
	}
	public set disabled(value) {
		this.#disabled = value;

		if (this._component) {
			this._component.disabled = this.#disabled;
		}
	}

	#href?: string;
	@property({ type: String, reflect: true })
	public get href() {
		return this.#href;
	}
	public set href(value) {
		this.#href = value;

		if (this._component) {
			this._component.href = this.#href;
		}
	}

	#detailProperties?: Array<UmbCollectionItemDetailPropertyConfig>;
	@property({ type: Array, attribute: false })
	public get detailProperties() {
		return this.#detailProperties;
	}
	public set detailProperties(value) {
		this.#detailProperties = value;

		if (this._component) {
			this._component.detailProperties = this.#detailProperties;
		}
	}

	#pathAddendum = new UmbRoutePathAddendumContext(this);

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		const unique = this.item?.unique;
		if (!unique) throw new Error('No unique id found for item');
		this.dispatchEvent(new UmbSelectedEvent(unique));
	}

	#onDeselected(event: UmbDeselectedEvent) {
		event.stopPropagation();
		const unique = this.item?.unique;
		if (!unique) throw new Error('No unique id found for item');
		this.dispatchEvent(new UmbDeselectedEvent(unique));
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, this.getMarkAttributeName());
	}

	#boundOnSelected = this.#onSelected.bind(this);
	#boundOnDeselected = this.#onDeselected.bind(this);

	#createController(entityType: string) {
		if (this.#extensionsController) {
			this.#extensionsController.destroy();
		}

		this.#extensionsController = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			this.getExtensionType(),
			(manifest: ManifestEntityCollectionItemBase) => manifest.forEntityTypes.includes(entityType),
			(extensionControllers) => {
				this._component?.remove();
				const component = extensionControllers[0]?.component || this.createFallbackElement();

				// TODO: I would say this code can use feature of the UmbExtensionsElementInitializer, to set properties and get a fallback element. [NL]
				// assign the properties to the component
				component.item = this.item;
				component.selectable = this.selectable;
				component.selectOnly = this.selectOnly;
				component.selected = this.selected;
				component.disabled = this.disabled;
				component.href = this.href;
				component.detailProperties = this.detailProperties;

				component.addEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
				component.addEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);

				// Proxy the actions slot to the component
				const slotElement = document.createElement('slot');
				slotElement.name = 'actions';
				slotElement.setAttribute('slot', 'actions');
				component.appendChild(slotElement);

				this._component = component;
			},
			undefined, // We can leave the alias to undefined, as we destroy this ourselves.
			undefined,
			{ single: true },
		);
	}

	override destroy(): void {
		this._component?.removeEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
		this._component?.removeEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);
		super.destroy();
	}

	/**
	 * Abstract methods that subclasses must implement
	 */
	protected abstract getExtensionType(): string;
	protected abstract createFallbackElement(): HTMLElement;
	protected abstract getPathAddendum(entityType: string, unique: string): string;
	protected abstract getMarkAttributeName(): string;
}
