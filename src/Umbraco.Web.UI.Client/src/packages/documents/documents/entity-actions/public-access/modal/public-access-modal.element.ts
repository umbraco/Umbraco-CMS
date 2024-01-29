import { UmbDocumentPublicAccessRepository } from '../repository/public-access.repository.js';
import { html, customElement, state, css, repeat, query } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import { UmbLanguageRepository } from '@umbraco-cms/backoffice/language';
import type {
	DomainPresentationModel,
	LanguageResponseModel,
	PublicAccessResponseModel,
} from '@umbraco-cms/backoffice/backend-api';
/*import {
	UmbDocumentPublicAccessRepository,
	type UmbPublicAccessModalData,
	type UmbPublicAccessModalValue,
} from '@umbraco-cms/backoffice/document';*/
import type { UmbPublicAccessModalData, UmbPublicAccessModalValue } from '@umbraco-cms/backoffice/document';

@customElement('umb-public-access-modal')
export class UmbPublicAccessModalElement extends UmbModalBaseElement<
	UmbPublicAccessModalData,
	UmbPublicAccessModalValue
> {
	@state()
	private _userMember?: any;

	@state()
	private _page?: string;

	@state()
	private _responseModel?: PublicAccessResponseModel;

	// Init

	firstUpdated() {
		this._responseModel = this.data?.data;
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

	// Renders

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_protect')}>
				Public Access
				<uui-button
					slot="actions"
					id="cancel"
					label=${this.localize.term('buttons_confirmActionCancel')}
					@click="${this.#handleCancel}"></uui-button>
				<uui-button
					slot="actions"
					id="save"
					look="primary"
					color="positive"
					label=${this.localize.term('buttons_save')}
					@click="${this.#handleSave}"></uui-button>
			</umb-body-layout>
		`;
	}

	// First page when no Public Access Restricting is set.
	renderSelectGroup() {
		return html``;
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
