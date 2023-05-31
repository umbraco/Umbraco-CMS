import type { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
	UMB_ALLOWED_DOCUMENT_TYPES_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (this.unique) {
			await this._executeWithParent();
		} else {
			await this._executeAtRoot();
		}
	}

	private async _executeWithParent() {
		// TODO: what to do if modal service is not available?
		if (!this.repository) return;

		const { data } = await this.repository.requestById(this.unique);

		if (data && data.contentTypeId) {
			this._openModal(data.contentTypeId);
		}
	}

	private async _executeAtRoot() {
		this._openModal(null);
	}

	private async _openModal(documentId: string | null) {
		if (!this.#modalContext) return;
		const modalHandler = this.#modalContext.open(UMB_ALLOWED_DOCUMENT_TYPES_MODAL, {
			id: documentId,
		});

		const { documentTypeKey } = await modalHandler.onSubmit();

		if (this.unique) {
			// TODO: how do we want to generate these urls?
			history.pushState(null, '', `section/content/workspace/document/create/${this.unique}/${documentTypeKey}`);
		} else {
			history.pushState(null, '', `section/content/workspace/document/create/null/${documentTypeKey}`);
		}
	}
}
