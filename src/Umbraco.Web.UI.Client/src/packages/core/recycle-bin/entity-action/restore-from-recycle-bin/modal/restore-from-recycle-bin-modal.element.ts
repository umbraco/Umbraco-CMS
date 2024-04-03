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
	_customSelectDestination = false;

	@state()
	_destinationItem: any;

	constructor() {
		super();
	}

	protected async firstUpdated(_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>): Promise<void> {
		super.firstUpdated(_changedProperties);

		const restoreDestination = await this.#requestRestoreDestination();

		if (!restoreDestination) {
			this._customSelectDestination = true;
			this.requestUpdate();
			return;
		}
	}

	async #requestRestoreDestination(): Promise<string | undefined> {
		if (!this.data?.unique) throw new Error('Cannot restore an item without a unique identifier.');
		if (!this.data?.recycleBinRepositoryAlias)
			throw new Error('Cannot restore an item without a recycle bin repository alias.');

		const recycleBinRepository = await createExtensionApiByAlias<UmbRecycleBinRepository>(
			this,
			this.data.recycleBinRepositoryAlias,
		);

		const { data } = await recycleBinRepository.requestOriginalParent({
			unique: this.data.unique,
		});

		// The original parent is still available. We will restore to that
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

			console.log(data);
			debugger;
		}
	}

	async requestDestinationItem(unique: string | null) {
		if (!this.data?.itemRepositoryAlias) throw new Error('Cannot restore an item without an item repository alias.');

		const itemRepository = await createExtensionApiByAlias<UmbItemRepository<any>>(this, this.data.itemRepositoryAlias);
		const { data } = await itemRepository.requestItems([unique]);
	}

	render() {
		return html`
			<umb-body-layout headline="Restore">
				<uui-box>
					${this._customSelectDestination
						? html`
								<h4>Cannot automatically restore this item.</h4>
								<p>
									There is no location where this item can be automatically restored. You can select a new location
									below.
								</p>

								<uui-button look="secondary" @click=${this.#onSelectCustomDestination}>Select location</uui-button>
							`
						: nothing}
				</uui-box>
			</umb-body-layout>
		`;
	}

	#renderDestination() {}

	#renderCustomSelection() {}

	static styles = [UmbTextStyles, css``];
}

export default UmbRestoreFromRecycleBinModalElement;

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbRestoreFromRecycleBinModalElement;
	}
}
