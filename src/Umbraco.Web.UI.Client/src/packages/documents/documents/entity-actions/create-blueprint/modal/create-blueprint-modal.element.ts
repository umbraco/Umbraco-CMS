import { UmbDocumentDetailRepository } from '../../../repository/index.js';
import type { UmbCreateBlueprintModalData } from './create-blueprint-modal.token.js';
import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';
import {
	type UmbDocumentBlueprintDetailModel,
	UmbDocumentBlueprintDetailRepository,
} from '@umbraco-cms/backoffice/document-blueprint';
import type { UUIInputEvent } from '@umbraco-cms/backoffice/external/uui';

@customElement('umb-create-blueprint-modal')
export class UmbCreateBlueprintModalElement extends UmbModalBaseElement<UmbCreateBlueprintModalData, never> {
	#blueprintRepository = new UmbDocumentBlueprintDetailRepository(this);
	#documentRepository = new UmbDocumentDetailRepository(this);

	#documentUnique = '';
	#document?: UmbDocumentDetailModel;

	@state()
	private _documentName = '';

	@state()
	private _blueprintName = '';

	firstUpdated() {
		this.#documentUnique = this.data?.unique ?? '';
		this.#getDocumentData();
	}

	async #getDocumentData() {
		const { data } = await this.#documentRepository.requestByUnique(this.#documentUnique);
		if (!data) return;

		this.#document = data;
		this._documentName = data.variants[0].name;
		this._blueprintName = data.variants[0].name;
	}

	#mapDocumentToBlueprintModel() {
		if (!this.#document) return;
		const variants = { ...this.#document?.variants, name: this._blueprintName };
		const model: UmbDocumentBlueprintDetailModel = { ...this.#document, entityType: 'document-blueprint', variants };

		return model;
	}

	async #handleSave() {
		const model = this.#mapDocumentToBlueprintModel();
		if (!model) return;
		const { error } = await this.#blueprintRepository.create(model, null);
		console.log('error', error);
		this.modalContext?.submit();
	}

	#renderBlueprintName() {
		return html`<strong>Create a new Content Template from ${this._documentName}</strong>
			A Content Template is predefined content that an editor can select to use as the basis for creating new content .
			<uui-label for="name">Name</uui-label>
			<uui-input
				id="name"
				label="name"
				.value=${this._blueprintName}
				@input=${(e: UUIInputEvent) => (this._blueprintName = e.target.value as string)}></uui-input>`;
	}

	render() {
		return html`
			<umb-body-layout headline="Create Content Template">
				${this.#renderBlueprintName()}
				<uui-button
					slot="actions"
					id="close"
					label=${this.localize.term('general_close')}
					@click="${this.#handleClose}"></uui-button>
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

	#handleClose() {
		this.modalContext?.reject();
	}

	static styles = [
		UmbTextStyles,
		css`
			strong,
			uui-label,
			uui-input {
				display: block;
			}

			uui-label {
				margin-top: var(--uui-size-space-6);
			}
		`,
	];
}

export default UmbCreateBlueprintModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-create-blueprint-modal': UmbCreateBlueprintModalElement;
	}
}
