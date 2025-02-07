import type { UmbEntityModel } from '../types.js';
import type { ManifestEntityItemRef } from './entity-item-ref.extension.js';
import { customElement, property, type PropertyValueMap, state, css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-entity-item-ref')
export class UmbEntityItemRefElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementInitializer<any>;
	#item?: UmbEntityModel;

	@state()
	_component?: HTMLElement;

	@property({ type: Object, attribute: false })
	public get item(): UmbEntityModel | undefined {
		return this.#item;
	}
	public set item(value: UmbEntityModel | undefined) {
		if (value === this.#item || !value) return;
		this.#item = value;
		this.#createController(value.entityType);
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
				const component = extensionControllers[0]?.component;

				// assign the item to the component
				component.item = this.#item;

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
