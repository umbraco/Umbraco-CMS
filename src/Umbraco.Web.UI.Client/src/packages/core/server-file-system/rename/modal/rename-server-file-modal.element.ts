import type { UmbRenameServerFileRepository } from '../types.js';
import type { UmbRenameModalData, UmbRenameServerFileModalValue } from './rename-server-file-modal.token.js';
import type { PropertyValueMap } from '@umbraco-cms/backoffice/external/lit';
import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { umbFocus } from '@umbraco-cms/backoffice/lit-element';

@customElement('umb-rename-modal')
export class UmbRenameModalElement extends UmbModalBaseElement<UmbRenameModalData, UmbRenameServerFileModalValue> {
	// TODO: make base type for item and detail models
	#itemRepository?: UmbItemRepository<any>;
	#renameRepository?: UmbRenameServerFileRepository<any>;
	#init?: Promise<unknown>;

	@state()
	_name = '';

	override connectedCallback(): void {
		super.connectedCallback();
		this.#observeRepository();
	}

	#observeRepository() {
		if (!this.data?.renameRepositoryAlias) throw new Error('A rename repository alias is required');
		if (!this.data?.itemRepositoryAlias) throw new Error('An item repository alias is required');

		// TODO: We should properly look into how we can simplify the one time usage of a extension api, as its a bit of overkill to take conditions/overwrites and observation of extensions into play here: [NL]
		// But since this happens when we execute an action, it does most likely not hurt any users, but it is a bit of a overkill to do this for every action: [NL]
		this.#init = Promise.all([
			new UmbExtensionApiInitializer(
				this,
				umbExtensionsRegistry,
				this.data.itemRepositoryAlias,
				[this],
				(permitted, ctrl) => {
					this.#itemRepository = permitted ? (ctrl.api as UmbItemRepository<any>) : undefined;
				},
			).asPromise(),

			new UmbExtensionApiInitializer(
				this,
				umbExtensionsRegistry,
				this.data.renameRepositoryAlias,
				[this],
				(permitted, ctrl) => {
					this.#renameRepository = permitted ? (ctrl.api as UmbRenameServerFileRepository<any>) : undefined;
				},
			).asPromise(),
		]);
	}

	protected override async firstUpdated(
		_changedProperties: PropertyValueMap<any> | Map<PropertyKey, unknown>,
	): Promise<void> {
		super.firstUpdated(_changedProperties);
		if (!this.data?.unique) throw new Error('Unique identifier is not available');
		await this.#init;
		if (!this.#itemRepository) throw new Error('Item repository is not available');

		const { data } = await this.#itemRepository.requestItems([this.data.unique]);
		this._name = data?.[0].name ?? '';
	}

	async #onSubmit(event: SubmitEvent) {
		event.preventDefault();
		if (!this.#renameRepository) throw new Error('Rename repository is not available');
		if (!this.data?.unique) throw new Error('Unique identifier is not available');

		const form = event.target as HTMLFormElement;
		if (!form) return;

		const isValid = form.checkValidity();
		if (!isValid) return;

		const formData = new FormData(form);
		const name = formData.get('name') as string;

		const { data } = await this.#renameRepository.rename(this.data.unique, name);

		if (data) {
			this.value = {
				name: data.name,
				unique: data.unique,
			};

			this._submitModal();
		} else {
			this._rejectModal();
		}
	}

	override render() {
		return html`
			<umb-body-layout headline=${'Rename'}>
				<uui-box>
					<uui-form>
						<form id="RenameForm" @submit="${this.#onSubmit}">
							<uui-form-layout-item>
								<uui-label id="nameLabel" for="name" slot="label" required>Name</uui-label>
								<uui-input
									type="text"
									id="name"
									name="name"
									value=${this._name}
									placeholder="Enter new name..."
									required
									required-message="Name is required"
									${umbFocus()}></uui-input>
							</uui-form-layout-item>
						</form>
					</uui-form>
				</uui-box>

				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button
					form="RenameForm"
					type="submit"
					slot="actions"
					color="positive"
					look="primary"
					label="Rename"></uui-button>
			</umb-body-layout>
		`;
	}

	static override styles = [
		UmbTextStyles,
		css`
			#name {
				width: 100%;
			}
		`,
	];
}

export default UmbRenameModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-rename-modal': UmbRenameModalElement;
	}
}
