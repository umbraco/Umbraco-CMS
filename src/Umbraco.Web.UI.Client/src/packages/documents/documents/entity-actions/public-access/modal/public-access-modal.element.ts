import { html, customElement, state, css, repeat, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbLanguageRepository } from '@umbraco-cms/backoffice/language';
import type { DomainPresentationModel, LanguageResponseModel } from '@umbraco-cms/backoffice/backend-api';
/*import {
	UmbDocumentPublicAccessRepository,
	type UmbPublicAccessModalData,
	type UmbPublicAccessModalValue,
} from '@umbraco-cms/backoffice/document';*/
import type { UUIInputEvent, UUIPopoverContainerElement, UUISelectEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-public-access-modal')
export class UmbPublicAccessModalElement extends UmbModalBaseElement<any, any> {
	#publicAccessRepository?: any; //new UmbDocumentPublicAccessRepository(this);
	#unique?: string;
	#model?: any;

	// Init

	firstUpdated() {
		this.#unique = this.data?.unique;
		this.#requestPublicAccessModel();
	}

	async #requestPublicAccessModel() {
		if (!this.#unique) return;
		const { data } = await this.#publicAccessRepository.requestPublicAccessModel(this.#unique);

		if (!data) return;
		this.#model = data;
	}

	// Modal

	async #handleSave() {
		//this.value = { defaultIsoCode: this._defaultIsoCode, domains: this._domains };
		//const { error } = await this.#documentRepository.updateCultureAndHostnames(this.#unique!, this.value);
		//if (error) return;
		this.modalContext?.submit();
	}

	#handleCancel() {
		this.modalContext?.reject();
	}

	render() {
		return html`Public Access`;
	}

	static styles = [
		UmbTextStyles,
		css`
			uui-button-group {
				width: 100%;
			}
			uui-box:first-child {
				margin-bottom: var(--uui-size-layout-1);
			}
			#dropdown {
				flex-grow: 0;
			}
			#domains {
				margin-top: var(--uui-size-layout-1);
				margin-bottom: var(--uui-size-2);
				display: grid;
				grid-template-columns: 1fr 1fr auto;
				grid-gap: var(--uui-size-1);
			}
		`,
	];
}

export default UmbPublicAccessModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-public-access-modal': UmbPublicAccessModalElement;
	}
}
