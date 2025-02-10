import type { UmbTrashConfirmModalData, UmbTrashConfirmModalValue } from './trash-confirm-modal.token.js';
import { html, customElement, property, css, state, nothing } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { UmbLitElement, umbFocus } from '@umbraco-cms/backoffice/lit-element';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-trash-confirm-modal')
export class UmbTrashConfirmModalElement extends UmbLitElement {
	@property({ attribute: false })
	modalContext?: UmbModalContext<UmbTrashConfirmModalData, UmbTrashConfirmModalValue>;

	@property({ type: Object, attribute: false })
	private _data?: UmbTrashConfirmModalData | undefined;
	public get data(): UmbTrashConfirmModalData | undefined {
		return this._data;
	}
	public set data(value: UmbTrashConfirmModalData | undefined) {
		this._data = value;
		this.#initData();
	}

	private _handleConfirm() {
		this.modalContext?.submit();
	}

	private _handleCancel() {
		this.modalContext?.reject();
	}

	@state()
	_item: any;

	@state()
	_referencedBy: any[] = [];

	@state()
	_totalReferencedBy: number = 0;

	#itemRepository?: UmbItemRepository<any>;
	#referenceRepository?: any;

	#limitReferencedBy = 3;

	constructor() {
		super();
		this.#initData();
	}

	async #initData() {
		if (!this._data) {
			this.#itemRepository?.destroy();
			this.#referenceRepository?.destroy();
			return;
		}

		this.#itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(
			this,
			this._data.itemRepositoryAlias,
		);

		const { data } = await this.#itemRepository.requestItems([this._data.unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item not found.');

		this._item = item.name;

		this.#loadReferencedBy();
	}

	async #loadReferencedBy() {
		// Skip if there is no reference repository
		if (!this._data?.referenceRepositoryAlias) return;

		this.#referenceRepository = await createExtensionApiByAlias<any>(this, this._data.referenceRepositoryAlias);

		const { data: referencesData } = await this.#referenceRepository.requestReferencedBy(
			this._data.unique,
			0,
			this.#limitReferencedBy,
		);

		if (referencesData) {
			this._referencedBy = [...referencesData.items];
			this._totalReferencedBy = referencesData.total;
		}
	}

	override render() {
		return html`
			<uui-dialog-layout class="uui-text" headline="Trash">
				<p>Are you sure you want to move <strong>${this._item}</strong> to the recycle bin?</p>
				${this.#renderReferencedBy()}

				<uui-button slot="actions" id="cancel" label="Cancel" @click=${this._handleCancel}></uui-button>

				<uui-button
					slot="actions"
					id="confirm"
					color="danger"
					look="primary"
					label="Trash"
					@click=${this._handleConfirm}
					${umbFocus()}></uui-button>
			</uui-dialog-layout>
		`;
	}

	#renderReferencedBy() {
		if (this._totalReferencedBy === 0) return nothing;

		return html`
			<h5 id="reference-headline">${this.localize.term('references_labelDependsOnThis')}</h5>
			<uui-ref-list>
				${this._referencedBy.map(
					(reference) => html`<umb-entity-item-ref .item=${reference} readonly></umb-entity-item-ref> `,
				)}
			</uui-ref-list>
			${this._totalReferencedBy > this.#limitReferencedBy
				? html`<span
						>${this.localize.term(
							'references_labelMoreReferences',
							this._totalReferencedBy - this.#limitReferencedBy,
						)}</span
					>`
				: nothing}
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			uui-dialog-layout {
				max-inline-size: 60ch;
			}

			#reference-headline {
				margin-bottom: var(--uui-size-3);
			}

			uui-ref-list {
				margin-bottom: var(--uui-size-2);
			}
		`,
	];
}

export { UmbTrashConfirmModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		'umb-trash-confirm-modal': UmbTrashConfirmModalElement;
	}
}
