import type {
	UmbDocumentBlueprintCreateOptionsModalData,
	UmbDocumentBlueprintCreateOptionsModalValue,
} from './index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { html, customElement, css, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbModalBaseElement } from '@umbraco-cms/backoffice/modal';
import type { DocumentTypeResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbDocumentTypeStructureRepository } from '@umbraco-cms/backoffice/document-type';
import { type UmbSelectedEvent, UmbSelectionChangeEvent } from '@umbraco-cms/backoffice/event';
import type { UmbTreeElement } from '@umbraco-cms/backoffice/tree';

@customElement('umb-document-blueprint-create-options-modal')
export class UmbDocumentBlueprintCreateOptionsModalElement extends UmbModalBaseElement<
	UmbDocumentBlueprintCreateOptionsModalData,
	UmbDocumentBlueprintCreateOptionsModalValue
> {
	@state()
	private _documentTypes?: Array<DocumentTypeResponseModel>;

	#documentTypeRepository = new UmbDocumentTypeStructureRepository(this);

	connectedCallback(): void {
		super.connectedCallback();
	}

	async #fetchTypes() {
		//const something = await this.#documentTypeRepository.
	}

	#onNavigate() {
		this._submitModal();
	}

	#onSelected(event: UmbSelectedEvent) {
		event.stopPropagation();
		const element = event.target as UmbTreeElement;
		this.value = { documentTypeUnique: element.getSelection()[0] };
		this.modalContext?.dispatchEvent(new UmbSelectionChangeEvent());
		this.#onNavigate();
	}

	render() {
		return html`
			<umb-body-layout headline=${this.localize.term('actions_createblueprint')}>
				<uui-box>
					<strong>Create an item under Content Templates</strong>
					<umb-localize key="create_createContentBlueprint">
						Select the Document Type you want to make a content blueprint for
					</umb-localize>
					<umb-tree
						alias="Umb.Tree.DocumentType"
						.props=${{
							hideTreeRoot: true,
							selectableFilter: (item: any) => item.isElement == false,
						}}
						@selected=${this.#onSelected}></umb-tree>
				</uui-box>
				<uui-button slot="actions" id="cancel" label="Cancel" @click="${this._rejectModal}"></uui-button>
			</umb-body-layout>
		`;
	}

	static styles = [
		UmbTextStyles,
		css`
			strong {
				display: block;
			}
		`,
	];
}

export default UmbDocumentBlueprintCreateOptionsModalElement;

declare global {
	interface HTMLElementTagNameMap {
		'umb-document-blueprint-create-options-modal': UmbDocumentBlueprintCreateOptionsModalElement;
	}
}
