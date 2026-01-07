import type { UmbDropdownElement } from '../../../components/dropdown/index.js';
import { UmbEntityActionListElement } from '../../entity-action-list.element.js';
import { html, customElement, property, css, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UUIScrollContainerElement } from '@umbraco-cms/backoffice/external/uui';
import { UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { observeMultiple } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-entity-actions-dropdown')
export class UmbEntityActionsDropdownElement extends UmbLitElement {
	@property({ type: Boolean })
	compact = false;

	@property({ type: String })
	public label?: string;

	@query('#action-modal')
	private _dropdownElement?: UmbDropdownElement;

	#scrollContainerElement?: UUIScrollContainerElement;
	#entityActionListElement?: UmbEntityActionListElement;
	#entityType?: UmbEntityModel['entityType'];
	#unique?: UmbEntityModel['unique'];

	constructor() {
		super();
		this.consumeContext(UMB_ENTITY_CONTEXT, (context) => {
			if (!context) return;

			this.observe(observeMultiple([context.entityType, context.unique]), ([entityType, unique]) => {
				this.#entityType = entityType;
				this.#unique = unique;

				if (this.#entityActionListElement) {
					this.#entityActionListElement.entityType = entityType;
					this.#entityActionListElement.unique = unique;
				}
			});
		});
	}

	#onActionExecuted() {
		this._dropdownElement?.closeDropdown();
	}

	#onDropdownClick(event: Event) {
		event.stopPropagation();
	}

	#onDropdownOpened() {
		if (this.#entityActionListElement) {
			this.#entityActionListElement.focus();
			return;
		}

		// First create dropdown content when the dropdown is opened.
		// Programmatically create the elements so they are cached if the dropdown is opened again
		this.#scrollContainerElement = new UUIScrollContainerElement();
		this.#entityActionListElement = new UmbEntityActionListElement();
		this.#entityActionListElement.addEventListener('action-executed', this.#onActionExecuted.bind(this));
		this.#entityActionListElement.entityType = this.#entityType;
		this.#entityActionListElement.unique = this.#unique;
		this.#entityActionListElement.setAttribute('label', this.label ?? '');
		this.#scrollContainerElement.appendChild(this.#entityActionListElement);
		this._dropdownElement?.appendChild(this.#scrollContainerElement);
	}

	override render() {
		return html`<umb-dropdown
			id="action-modal"
			@click=${this.#onDropdownClick}
			@opened=${this.#onDropdownOpened}
			@focusout=${(e: FocusEvent) => {
				const relatedTarget = e.relatedTarget as HTMLElement | null;
				if (!relatedTarget) return;
				const allowedTags = ['umb-entity-action-list', 'uui-scroll-container', 'uui-menu-item', 'umb-entity-action'];
				if (allowedTags.includes(relatedTarget.tagName.toLowerCase())) return;
				this.#onActionExecuted();
			}}
			.label=${this.label}
			?compact=${this.compact}
			hide-expand>
			<slot name="label" slot="label"></slot>
			<slot></slot>
		</umb-dropdown>`;
	}

	static override styles = [
		css`
			uui-scroll-container {
				max-height: 700px;
			}
		`,
	];
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-entity-actions-dropdown': UmbEntityActionsDropdownElement;
	}
}
