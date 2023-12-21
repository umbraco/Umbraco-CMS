import {
	DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
	type UmbDocumentTypeTreeItemModel,
} from '@umbraco-cms/backoffice/document-type';
import { UmbDeleteEvent } from '@umbraco-cms/backoffice/event';
import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UMB_CONFIRM_MODAL, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UmbRepositoryItemsManager } from '@umbraco-cms/backoffice/repository';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';

@customElement('umb-block-type-card')
export class UmbBlockTypeCardElement extends UmbLitElement {
	//
	// TODO: Look at ways to avoid using backend models for this implementation, could we get it from the Repository, eventually we need to invent a Repository Token.
	#itemManager = new UmbRepositoryItemsManager<UmbDocumentTypeTreeItemModel>(
		this,
		DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		(x) => x.id,
	);

	@property({ type: String, attribute: false })
	workspacePath?: string;

	@property({ type: String, attribute: false })
	public get key(): string | undefined {
		return this._key;
	}
	public set key(value: string | undefined) {
		this._key = value;
		if (value) {
			this.#itemManager.setUniques([value]);
		} else {
			this.#itemManager.setUniques([]);
		}
	}
	private _key?: string | undefined;

	@state()
	_name?: string;

	@state()
	_icon?: string | null;

	constructor() {
		super();

		this.observe(this.#itemManager.items, (items) => {
			const item = items[0];
			if (item) {
				this._icon = item.icon;
				this._name = item.name;
			}
		});
	}

	#onRequestDelete() {
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, async (modalManager) => {
			const modalContext = modalManager.open(UMB_CONFIRM_MODAL, {
				data: {
					color: 'danger',
					headline: `Remove ${this._name}?`,
					content: 'Are you sure you want to remove this item',
					confirmLabel: 'Remove',
				},
			});

			await modalContext?.onSubmit();
			this.dispatchEvent(new UmbDeleteEvent());
		});
	}

	render() {
		return html`
			<uui-card-block-type href="${this.workspacePath}/edit/${this.key}" .name=${this._name ?? ''}>
				<uui-icon name=${this._icon ?? ''}></uui-icon>
				<uui-action-bar slot="actions">
					<uui-button @click=${this.#onRequestDelete} label="Remove block">
						<uui-icon name="icon-trash"></uui-icon>
					</uui-button>
				</uui-action-bar>
			</uui-card-block-type>
		`;
	}
}

export default UmbBlockTypeCardElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-block-type-card': UmbBlockTypeCardElement;
	}
}
