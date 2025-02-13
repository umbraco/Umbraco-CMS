import type { UmbEntityModel } from '../types.js';
import type { ManifestEntityItemRef } from './entity-item-ref.extension.js';
import { customElement, property, type PropertyValueMap, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

import './default-item-ref.element.js';

@customElement('umb-entity-item-ref')
export class UmbEntityItemRefElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementInitializer<any>;
	#item?: UmbEntityModel;

	@state()
	_component?: any; // TODO: Add type

	@property({ type: Object, attribute: false })
	public get item(): UmbEntityModel | undefined {
		return this.#item;
	}
	public set item(value: UmbEntityModel | undefined) {
		const oldValue = this.#item;
		this.#item = value;

		if (value === oldValue) return;
		if (!value) return;

		// If the component is already created and the entity type is the same, we can just update the item.
		if (this._component && value.entityType === oldValue?.entityType) {
			this._component.item = value;
			return;
		}

		// If the component is already created, but the entity type is different, we need to destroy the component.
		this.#createController(value.entityType);
	}

	#readonly = false;
	@property({ type: Boolean, attribute: 'readonly' })
	public get readonly() {
		return this.#readonly;
	}
	public set readonly(value) {
		this.#readonly = value;

		if (this._component) {
			this._component.readonly = this.#readonly;
		}
	}

	#standalone = false;
	@property({ type: Boolean, attribute: 'standalone' })
	public get standalone() {
		return this.#standalone;
	}
	public set standalone(value) {
		this.#standalone = value;

		if (this._component) {
			this._component.standalone = this.#standalone;
		}
	}

	protected override firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): void {
		super.firstUpdated(_changedProperties);
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'entity-item-ref');
	}

	#createController(entityType: string) {
		if (this.#extensionsController) {
			this.#extensionsController.destroy();
		}

		this.#extensionsController = new UmbExtensionsElementInitializer(
			this,
			umbExtensionsRegistry,
			'entityItemRef',
			(manifest: ManifestEntityItemRef) => manifest.forEntityTypes.includes(entityType),
			(extensionControllers) => {
				this._component?.remove();
				const component = extensionControllers[0]?.component || document.createElement('umb-default-item-ref');

				// assign the properties to the component
				component.item = this.#item;
				component.readonly = this.readonly;
				component.standalone = this.standalone;

				// Proxy the actions slot to the component
				const slotElement = document.createElement('slot');
				slotElement.name = 'actions';
				slotElement.setAttribute('slot', 'actions');
				component.appendChild(slotElement);

				this._component = component;
			},
			undefined, // We can leave the alias to undefined, as we destroy this our selfs.
			undefined,
			{ single: true },
		);
	}

	override render() {
		return html`${this._component}`;
	}

	static override styles = [
		css`
			:host {
				display: block;
				position: relative;
			}
		`,
	];
}

export { UmbEntityItemRefElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-item-ref': UmbEntityItemRefElement;
	}
}
