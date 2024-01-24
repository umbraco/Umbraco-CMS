import type { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT,
	UMB_CREATE_DOCUMENT_MODAL as UMB_CREATE_DOCUMENT_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;
		this._openModal(this.unique || null);
	}

	private async _openModal(id: string | null) {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;
		const modalContext = this.#modalContext.open(UMB_CREATE_DOCUMENT_MODAL, {
			data: {
				id,
			},
		});

		const { documentTypeId: documentTypeKey } = await modalContext.onSubmit();

		// TODO: how do we want to generate these urls?
		history.pushState(
			null,
			'',
			`section/content/workspace/document/create/${this.unique ?? 'null'}/${documentTypeKey}`,
		);
	}
}
