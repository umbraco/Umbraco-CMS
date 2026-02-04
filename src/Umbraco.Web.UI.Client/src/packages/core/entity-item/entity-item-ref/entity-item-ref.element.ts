import type { ManifestEntityItemRef } from './entity-item-ref.extension.js';
import { css, customElement, html, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbExtensionsElementInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDeselectedEvent, UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { UmbRoutePathAddendumContext } from '@umbraco-cms/backoffice/router';
import { UMB_MARK_ATTRIBUTE_NAME } from '@umbraco-cms/backoffice/const';
import { UUIBlinkAnimationValue } from '@umbraco-cms/backoffice/external/uui';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';

import './default-item-ref.element.js';

@customElement('umb-entity-item-ref')
export class UmbEntityItemRefElement extends UmbLitElement {
	#extensionsController?: UmbExtensionsElementInitializer<any>;
	#item?: UmbEntityModel;

	@state()
	private _component?: any; // TODO: Add type

	@property({ type: Object, attribute: false })
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

		this.#pathAddendum.setAddendum('ref/' + value.entityType + '/' + value.unique);

		// If the component is already created, but the entity type is different, we need to destroy the component.
		this.#createController(value.entityType);
	}
	public get item(): UmbEntityModel | undefined {
		return this.#item;
	}

	#readonly = false;
	@property({ type: Boolean, reflect: true })
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
	@property({ type: Boolean, reflect: true })
	public get standalone() {
		return this.#standalone;
	}
	public set standalone(value) {
		this.#standalone = value;

		if (this._component) {
			this._component.standalone = this.#standalone;
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

	@property({ type: Boolean })
	error?: boolean;

	@property({ type: String, attribute: 'error-message', reflect: false })
	errorMessage?: string | null;

	@property({ type: String, attribute: 'error-detail', reflect: false })
	errorDetail?: string | null;

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
		this.setAttribute(UMB_MARK_ATTRIBUTE_NAME, 'entity-item-ref');
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
			'entityItemRef',
			(manifest: ManifestEntityItemRef) => manifest.forEntityTypes.includes(entityType),
			(extensionControllers) => {
				this._component?.remove();
				const component = extensionControllers[0]?.component || document.createElement('umb-default-item-ref');

				// TODO: I would say this code can use feature of the UmbExtensionsElementInitializer, to set properties and get a fallback element. [NL]
				// assign the properties to the component
				component.item = this.item;
				component.readonly = this.readonly;
				component.standalone = this.standalone;
				component.selectOnly = this.selectOnly;
				component.selectable = this.selectable;
				component.selected = this.selected;
				component.disabled = this.disabled;

				component.addEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
				component.addEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);

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
		if (this._component) {
			return html`${this._component}`;
		}

		// Error:
		if (this.error) {
			return html`
				<uui-ref-node
					style="color: var(--uui-color-danger);"
					.name=${this.localize.string(this.errorMessage ?? '#general_notFound')}
					.detail=${this.errorDetail ?? ''}
					.readonly=${this.readonly}
					.standalone=${this.standalone}
					.selectOnly=${this.selectOnly}
					.selected=${this.selected}
					.disabled=${this.disabled}>
					<uui-icon slot="icon" name="icon-alert" style="color: var(--uui-color-danger);"></uui-icon>
					<slot name="actions"></slot>
				</uui-ref-node>
			`;
		}

		// Loading:
		return html`<uui-loader-bar id="loader"></uui-loader-bar>`;
	}

	override destroy(): void {
		this._component?.removeEventListener(UmbSelectedEvent.TYPE, this.#boundOnSelected);
		this._component?.removeEventListener(UmbDeselectedEvent.TYPE, this.#boundOnDeselected);
		super.destroy();
	}

	static override styles = [
		css`
			:host {
				display: block;
				position: relative;
			}

			#loader {
				margin-top: 10px;
				opacity: 0;
				animation: show-loader 0s 120ms forwards;
			}

			@keyframes show-loader {
				to {
					opacity: 1;
				}
			}

			:host::after {
				content: '';
				position: absolute;
				z-index: 1;
				pointer-events: none;
				inset: 0;
				border: 1px solid transparent;
				border-radius: var(--uui-border-radius);

				transition: border-color 240ms ease-in;
			}

			:host([drag-placeholder]) {
				--uui-color-focus: transparent;
			}

			:host([drag-placeholder])::after {
				display: block;
				border-width: 2px;
				border-color: var(--uui-color-interactive-emphasis);
				animation: ${UUIBlinkAnimationValue};
			}
			:host([drag-placeholder])::before {
				content: '';
				position: absolute;
				pointer-events: none;
				inset: 0;
				border-radius: var(--uui-border-radius);
				background-color: var(--uui-color-interactive-emphasis);
				opacity: 0.12;
			}
			:host([drag-placeholder]) > * {
				transition: opacity 50ms 16ms;
				opacity: 0;
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
