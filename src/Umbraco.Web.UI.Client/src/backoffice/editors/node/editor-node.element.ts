import { UUITextStyles } from '@umbraco-ui/uui-css/lib';
import { css, html, LitElement } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { UUIInputElement, UUIInputEvent } from '@umbraco-ui/uui';
import { UmbContextConsumerMixin, UmbContextProviderMixin } from '../../../core/context';
import { UmbNodeStore } from '../../../core/stores/node.store';
import { distinctUntilChanged, Subscription } from 'rxjs';
import { NodeEntity } from '../../../mocks/data/content.data';
import { UmbNotificationService } from '../../../core/services/notification.service';
import { UmbNodeContext } from './node.context';

import '../shared/editor-entity.element';

// Lazy load
// TODO: Make this dynamic, use load-extensions method to loop over extensions for this node.
import './views/editor-view-node-edit.element';
import './views/editor-view-node-info.element';

@customElement('umb-editor-node')
export class UmbEditorNodeElement extends UmbContextProviderMixin(UmbContextConsumerMixin(LitElement)) {
	static styles = [
		UUITextStyles,
		css`
			:host {
				display: block;
				width: 100%;
				height: 100%;
			}

			uui-input {
				width: 100%;
			}
		`,
	];

	@property()
	id!: string;

	@state()
	_node?: NodeEntity;

	private _nodeStore?: UmbNodeStore;
	private _nodeStoreSubscription?: Subscription;

	private _nodeContext?: UmbNodeContext;
	private _nodeContextSubscription?: Subscription;

	private _notificationService?: UmbNotificationService;

	constructor() {
		super();

		this.consumeContext('umbNodeStore', (store: UmbNodeStore) => {
			this._nodeStore = store;
			this._useNode();
		});

		this.consumeContext('umbNotificationService', (service: UmbNotificationService) => {
			this._notificationService = service;
		});

		this.addEventListener('property-value-change', this._onPropertyValueChange);
	}

	private _onPropertyValueChange = (e: Event) => {
		const target = e.composedPath()[0] as any;

		// TODO: Set value.
		const property = this._node?.properties.find((x) => x.alias === target.property.alias);
		if (property) {
			this._setPropertyValue(property.alias, target.value);
		} else {
			console.error('property was not found', target.property.alias);
		}
	};

	private _setPropertyValue(alias: string, value: unknown) {
		this._node?.data.forEach((data) => {
			if (data.alias === alias) {
				data.value = value;
			}
		});
	}

	private _useNode() {
		this._nodeStoreSubscription?.unsubscribe();

		this._nodeStoreSubscription = this._nodeStore?.getById(parseInt(this.id)).subscribe((node) => {
			if (!node) return; // TODO: Handle nicely if there is no node.

			this._nodeContextSubscription?.unsubscribe();

			if (!this._nodeContext) {
				this._nodeContext = new UmbNodeContext(node);
				this.provideContext('umbNodeContext', this._nodeContext);
			} else {
				this._nodeContext.update(node);
			}

			this._nodeContextSubscription = this._nodeContext.data.pipe(distinctUntilChanged()).subscribe((data) => {
				this._node = data;
			});
		});
	}

	private _onSaveAndPublish() {
		this._onSave();
	}

	private _onSave() {
		// TODO: What if store is not present, what if node is not loaded....
		if (this._node) {
			this._nodeStore?.save([this._node]).then(() => {
				this._notificationService?.peek('Document saved');
			});
		}
	}

	private _onSaveAndPreview() {
		this._onSave();
	}

	disconnectedCallback(): void {
		super.disconnectedCallback();
		this._nodeStoreSubscription?.unsubscribe();
		this._nodeContextSubscription?.unsubscribe();
		delete this._node;
	}

	// TODO. find a way where we don't have to do this for all editors.
	private _handleInput(event: UUIInputEvent) {
		if (event instanceof UUIInputEvent) {
			const target = event.composedPath()[0] as UUIInputElement;

			if (typeof target?.value === 'string') {
				this._nodeContext?.update({ name: target.value });
			}
		}
	}

	render() {
		return html`
			<umb-editor-entity alias="Umb.Editor.Node">
				<div slot="name">
					<uui-input .value=${this._node?.name} @input="${this._handleInput}"></uui-input>
					<!-- TODO: Implement variant selector -->
				</div>

				<div slot="footer">Breadcrumbs</div>

				<div slot="actions">
					<uui-button @click=${this._onSaveAndPreview} label="Save and preview"></uui-button>
					<uui-button @click=${this._onSave} look="secondary" label="Save"></uui-button>
					<uui-button
						@click=${this._onSaveAndPublish}
						look="primary"
						color="positive"
						label="Save and publish"></uui-button>
				</div>
			</umb-editor-entity>
		`;
	}
}

export default UmbEditorNodeElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-editor-node': UmbEditorNodeElement;
	}
}
