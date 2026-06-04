import type { UmbRecycleBinRepository } from '../../../recycle-bin-repository.interface.js';
import type {
	UmbRestoreFromRecycleBinModalData,
	UmbRestoreFromRecycleBinModalValue,
} from './restore-from-recycle-bin-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, state, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement, umbOpenModal } from '@umbraco-cms/backoffice/modal';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';

@customElement('umb-restore-from-recycle-bin-modal')
export class UmbRestoreFromRecycleBinModalElement extends UmbModalBaseElement<
	UmbRestoreFromRecycleBinModalData,
	UmbRestoreFromRecycleBinModalValue
> {
	@state()
	private _isAutomaticRestore = false;

	@state()
	private _destinationItem?: any;

	@state()
	private _destinationItemName?: string;

	@state()
	private _restoreItemName?: string;

	#recycleBinRepository?: UmbRecycleBinRepository;

	protected override async firstUpdated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.firstUpdated(_changedProperties);
		if (!this.data?.unique) throw new Error('Cannot restore an item without a unique identifier.');

		const restoreItem = await this.#requestItem(this.data.unique);

		if (restoreItem) {
			if (this.data.itemDataResolver) {
				const resolver = new this.data.itemDataResolver(this);
				resolver.setData(restoreItem);
				this._restoreItemName = await resolver.getName();
			} else {
				this._restoreItemName = restoreItem.name;
			}
		}

		const unique = await this.#requestAutomaticRestoreDestination();

		if (unique !== undefined) {
			this.setDestination(unique);
			this._isAutomaticRestore = true;
		}
	}

	async setDestination(unique: string | null) {
		// TODO: handle ROOT lookup. Currently, we can't look up the root in the item repository.
		// This is a temp solution to show something in the UI.
		if (unique === null) {
			this._destinationItemName = 'Root';
			this._destinationItem = null;

			this.#setDestinationValue({
				unique: null,
				entityType: this.data?.destinationRootEntityType ?? this.data?.entityType ?? 'unknown',
			});

			return;
		}

		if (unique) {
			this._destinationItem = await this.#requestDestinationItem(unique);
			if (!this._destinationItem) throw new Error('Cant find destination item.');

			this._destinationItemName = await this.#resolveDestinationItemName(this._destinationItem);

			this.#setDestinationValue({
				unique: this._destinationItem.unique,
				entityType: this._destinationItem.entityType,
			});
		}
	}

	async #resolveDestinationItemName(item: any): Promise<string> {
		const resolverCtor = this.data?.destinationItemDataResolver ?? this.data?.itemDataResolver;
		if (resolverCtor) {
			const resolver = new resolverCtor(this);
			resolver.setData(item);
			return (await resolver.getName()) ?? item.name;
		}
		return item.name;
	}

	async #requestAutomaticRestoreDestination(): Promise<string | null | undefined> {
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

		// only check for undefined because data can be null if the parent is the root
		if (data !== undefined) {
			return data ? data.unique : null;
		}

		return undefined;
	}

	async #requestItem(unique: string) {
		if (!this.data?.itemRepositoryAlias) throw new Error('Cannot restore an item without an item repository alias.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);
		const { data } = await itemRepository.requestItems([unique]);

		return data?.[0];
	}

	async #requestDestinationItem(unique: string) {
		const repoAlias = this.data?.destinationItemRepositoryAlias ?? this.data?.itemRepositoryAlias;
		if (!repoAlias) throw new Error('Cannot restore an item without an item repository alias.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, repoAlias);
		const { data } = await itemRepository.requestItems([unique]);

		return data?.[0];
	}

	async #onSelectCustomDestination() {
		if (!this.data?.pickerModal) throw new Error('Cannot select a destination without a picker modal.');

		const { selection } = await umbOpenModal(this, this.data.pickerModal, {
			data: {
				multiple: false,
			},
		});

		if (selection.length > 0) {
			const unique = selection[0];
			this.setDestination(unique);
		}
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

	#setDestinationValue(destination: UmbRestoreFromRecycleBinModalValue['destination']) {
		this.updateValue({ destination });
	}

	override render() {
		return html`
			<umb-body-layout headline="Restore">
				<uui-box>
					${this._isAutomaticRestore
						? html`Restore ${this._restoreItemName} to ${this._destinationItemName}`
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
			<h5>Restore to:</h5>
			${this._destinationItem !== undefined && this._destinationItemName
				? html`
						<uui-ref-node name=${this._destinationItemName}>
							<uui-action-bar slot="actions">
								<uui-button
									@click=${() => (this._destinationItem = undefined)}
									label=${this.localize.term('general_remove')}></uui-button>
							</uui-action-bar>
						</uui-ref-node>
					`
				: html`
						<uui-button
							id="placeholder"
							look="placeholder"
							label="Select location"
							@click=${this.#onSelectCustomDestination}></uui-button>
					`}
		`;
	}

	#renderActions() {
		return html`
			<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			<uui-button slot="actions" color="positive" look="primary" label="Restore" @click=${this.#onSubmit}></uui-button>
		`;
	}

	static override styles = [
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
		'umb-restore-from-recycle-bin-modal': UmbRestoreFromRecycleBinModalElement;
	}
}
