import type { UmbRenameRepository } from '../types.js';
import type { UmbRenameModalData, UmbRenameModalValue } from './rename-modal.token.js';
import { html, customElement, css } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbExtensionApiInitializer } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

@customElement('umb-rename-modal')
export class UmbRenameModalElement extends UmbModalBaseElement<UmbRenameModalData, UmbRenameModalValue> {
	#renameRepository?: UmbRenameRepository<any>;

	connectedCallback(): void {
		super.connectedCallback();
		this.#observeRepository();
	}

	#observeRepository() {
		if (!this.data?.renameRepositoryAlias) throw new Error('A rename repository alias is required');

		new UmbExtensionApiInitializer(
			this,
			umbExtensionsRegistry,
			this.data.renameRepositoryAlias,
			[this],
			(permitted, ctrl) => {
				this.#renameRepository = permitted ? (ctrl.api as UmbRenameRepository<any>) : undefined;
			},
		);
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
			this._submitModal();
		} else {
			this._rejectModal();
		}
	}

	render() {
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
									placeholder="Enter new name..."
									required
									required-message="Name is required"></uui-input>
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

	static styles = [
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
