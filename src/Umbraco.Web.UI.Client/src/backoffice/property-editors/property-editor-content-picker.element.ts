import { css, html, LitElement, nothing } from 'lit';
import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { customElement, property, state } from 'lit/decorators.js';

import { UmbContextConsumerMixin } from '../../core/context';
import { UmbModalService } from '../../core/services/modal';
import { UmbEntityStore } from '../../core/stores/entity.store';
import { Subscription } from 'rxjs';
import { NodeEntity } from '../../mocks/data/node.data';
import { Entity } from '../../mocks/data/entity.data';

@customElement('umb-property-editor-content-picker')
export class UmbPropertyEditorContentPicker extends UmbContextConsumerMixin(LitElement) {
	static styles = [
		UUITextStyles,
		css`
			h3 {
				margin-left: 16px;
				margin-right: 16px;
			}

			uui-input {
				width: 100%;
			}

			hr {
				border: none;
				border-bottom: 1px solid var(--uui-color-divider);
				margin: 16px 0;
			}

			#add-button {
				width: 100%;
			}
		`,
	];

	@property({ type: Array })
	public value: Array<string> = [];

	@state()
	private _items: Array<Entity> = [];

	private _modalService?: UmbModalService;
	private _entityStore?: UmbEntityStore;
	private _pickedEntitiesSubscription?: Subscription;

	constructor() {
		super();
		this.consumeContext('umbModalService', (modalService: UmbModalService) => {
			this._modalService = modalService;
		});

		this.consumeContext('umbEntityStore', (entityStore: UmbEntityStore) => {
			this._entityStore = entityStore;
			this._observePickedEntities();
		});
	}

	private _observePickedEntities() {
		this._pickedEntitiesSubscription?.unsubscribe();
		this._pickedEntitiesSubscription = this._entityStore?.getByKeys(this.value).subscribe((entities) => {
			this._items = entities;
		});
	}

	private _openPicker() {
		const modalHandler = this._modalService?.contentPicker({ multiple: true, selection: this.value });
		modalHandler?.onClose.then(({ selection }: any) => {
			this._setValue([...this.value, ...selection]);
		});
	}

	private _removeItem(item: Entity) {
		const modalHandler = this._modalService?.confirm({
			color: 'danger',
			headline: 'Remove',
			confirmLabel: 'Remove',
			content: html`Remove <strong>${item.name}</strong>?`,
		});

		modalHandler?.onClose.then(({ confirmed }) => {
			if (confirmed) {
				const newValue = this.value.filter((value) => value !== item.key);
				this._setValue(newValue);
			}
		});
	}

	private _setValue(newValue: Array<string>) {
		this.value = newValue;
		this._observePickedEntities();
		this.dispatchEvent(new CustomEvent('property-editor-change', { bubbles: true, composed: true }));
	}

	disconnectedCallback() {
		super.disconnectedCallback();
		this._pickedEntitiesSubscription?.unsubscribe();
	}

	private _renderItem(item: Entity) {
		return html`
			<uui-ref-node name=${item.name} detail=${item.key}>
				${item.isTrashed ? html` <uui-tag size="s" slot="tag" color="danger">Trashed</uui-tag> ` : nothing}
				<uui-action-bar slot="actions">
					<uui-button @click=${() => this._removeItem(item)}>Remove</uui-button>
				</uui-action-bar>
			</uui-ref-node>
		`;
	}

	render() {
		return html`${this._items.map((item) => this._renderItem(item))}
			<uui-button id="add-button" look="placeholder" @click=${this._openPicker} label="open">Add</uui-button>`;
	}
}

declare global {
	interface HTMLElementTagNameMap {
		'umb-property-editor-content-picker': UmbPropertyEditorContentPicker;
	}
}
