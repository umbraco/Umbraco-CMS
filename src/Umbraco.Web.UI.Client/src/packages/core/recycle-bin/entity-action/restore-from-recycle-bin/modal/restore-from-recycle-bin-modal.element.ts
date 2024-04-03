import type { UmbRecycleBinRepository } from '../../../recycle-bin-repository.interface.js';
import type {
	UmbRestoreFromRecycleBinModalData,
	UmbRestoreFromRecycleBinModalValue,
} from './restore-from-recycle-bin-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, nothing, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UMB_MODAL_MANAGER_CONTEXT, UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_DOCUMENT_PICKER_MODAL } from '@umbraco-cms/backoffice/document';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

const elementName = 'umb-restore-from-recycle-bin-modal';

@customElement(elementName)
export class UmbRestoreFromRecycleBinModalElement extends UmbModalBaseElement<
	UmbRestoreFromRecycleBinModalData,
	UmbRestoreFromRecycleBinModalValue
> {
	@state()
	_isAutomaticRestore = false;

	@state()
	_customSelectDestination = false;

	@state()
	_destinationItem?: any;

	#recycleBinRepository?: UmbRecycleBinRepository;

	protected async firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): Promise<void> {
		super.firstUpdated(_changedProperties);

		const restoreDestinationUnique = await this.#requestRestoreDestination();

		if (restoreDestinationUnique) {
			this._destinationItem = await this.#requestDestinationItem(restoreDestinationUnique);
			if (!this._destinationItem) throw new Error('Cant find destination item.');

			this.#setDestinationValue({
				unique: this._destinationItem.unique,
				entityType: this._destinationItem.entityType,
			});

			this._isAutomaticRestore = true;
		}
	}

	async #requestRestoreDestination(): Promise<string | undefined> {
		if (!this.data?.unique) throw new Error('Cannot restore an item without a unique identifier.');
		if (!this.data?.recycleBinRepositoryAlias)
			throw new Error('Cannot restore an item without a recycle bin repository alias.');

		this.#recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.data.recycleBinRepositoryAlias,
		);

		const { data } = await this.#recycleBinRepository.requestOriginalParent({
			unique: this.data.unique,
		});

		if (data) {
			return data.unique;
		}

		return undefined;
	}

	async #onSelectCustomDestination() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_DOCUMENT_PICKER_MODAL, {
			data: {
				multiple: false,
			},
		});

		const { selection } = await modal.onSubmit();

		if (selection.length > 0) {
			const destinationUnique = selection[0];
			this._destinationItem = await this.#requestDestinationItem(destinationUnique);
			if (!this._destinationItem) throw new Error('Cant find destination item.');

			this.#setDestinationValue({
				unique: this._destinationItem.unique,
				entityType: this._destinationItem.entityType,
			});
		}
	}

	async #requestDestinationItem(unique: string | null) {
		if (unique === null) {
			console.log('ROOT IS SELECTED, HANDLE THIS CASE');
			return;
		}

		if (!this.data?.itemRepositoryAlias) throw new Error('Cannot restore an item without an item repository alias.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);
		const { data } = await itemRepository.requestItems([unique]);

		return data?.[0];
	}

	async #onSubmit() {
		if (!this.value.destination) throw new Error('Cannot restore an item without a destination.');
		if (!this.#recycleBinRepository) throw new Error('Cannot restore an item without a destination.');
		if (!this.data?.unique) throw new Error('Cannot restore an item without a unique identifier.');

		const { error } = await this.#recycleBinRepository.requestRestore({
			unique: this.data.unique,
			destination: { unique: this.value.destination.unique },
		});

		if (!error) {
			this._submitModal();
		}
	}

	#setDestinationValue(destination: { unique: string; entityType: string }) {
		this.updateValue({ destination });
	}

	render() {
		return html`
			<umb-body-layout headline="Restore">
				<uui-box>
					${this._isAutomaticRestore
						? html` Restore (ITEM NAME HERE) to ${this._destinationItem.name}`
						: this.#renderCustomSelectDestination()}
				</uui-box>
				${this.#renderActions()}
			</umb-body-layout>
		`;
	}

	#renderCustomSelectDestination() {
		return html`
			<h4>Cannot automatically restore this item.</h4>
			<p>There is no location where this item can be automatically restored. You can select a new location below.</p>

			${this._destinationItem
				? html`<uui-ref-node name=${this._destinationItem.name}>
						<uui-action-bar slot="actions">
							<uui-button @click=${() => (this._destinationItem = undefined)} label="Remove"
								>${this.localize.term('general_remove')}</uui-button
							>
						</uui-action-bar>
					</uui-ref-node>`
				: html` <uui-button id="placeholder" look="placeholder" @click=${this.#onSelectCustomDestination}
						>Select location</uui-button
					>`}
		`;
	}

	#renderActions() {
		return html`
			<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			<uui-button slot="actions" color="positive" look="primary" label="Restore" @click=${this.#onSubmit}></uui-button>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			#placeholder {
				width: 100%;
			}
		`,
	];
}

export default UmbRestoreFromRecycleBinModalElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbRestoreFromRecycleBinModalElement;
	}
}
