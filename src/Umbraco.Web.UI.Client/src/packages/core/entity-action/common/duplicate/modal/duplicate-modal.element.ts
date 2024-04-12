import type { UmbDuplicateRepository } from '../duplicate-repository.interface.js';
import type { UmbDuplicateModalData, UmbDuplicateModalValue } from './duplicate-modal.token.js';
import { html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';

import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';

const elementName = 'umb-duplicate-modal';
@customElement(elementName)
export class UmbDuplicateModalElement extends UmbModalBaseElement<UmbDuplicateModalData, UmbDuplicateModalValue> {
	constructor() {
		super();
	}

	async #onSubmit(event: PointerEvent) {
		event?.stopPropagation();
		if (!this.data?.duplicateRepositoryAlias) throw new Error('duplicateRepositoryAlias is required');

		const duplicateRepository = await createExtensionApiByAlias<UmbDuplicateRepository>(
			this,
			this.data.duplicateRepositoryAlias,
		);

		const { error } = await duplicateRepository.requestDuplicateTo({
			unique: this.data.unique,
			destination: {
				unique: null,
			},
		});

		if (!error) {
			this._submitModal();
		}
	}

	render() {
		return html`
			<umb-body-layout headline="Duplicate">
				<div>Render Picker</div>

				<div>Render checkbox 1</div>
				<div>Render checkbox 2</div>

				<uui-button slot="actions" label="Cancel" @click="${this._rejectModal}"></uui-button>
				<uui-button slot="actions" color="positive" look="primary" label="Sort" @click=${this.#onSubmit}></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [UmbTextStyles];
}

export { UmbDuplicateModalElement as element };

declare global {
	interface HTMLElementTagNameMap {
		[elementName]: UmbDuplicateModalElement;
	}
}
