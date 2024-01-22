import { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalManagerContext, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UMB_CULTURE_AND_HOSTNAMES_MODAL } from '@umbraco-cms/backoffice/document';

export class UmbDocumentCultureAndHostnamesEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;
		this._openModal(this.unique || null);
	}

	private async _openModal(unique: string | null) {
		if (!this.#modalContext) return;
		this.#modalContext.open(UMB_CULTURE_AND_HOSTNAMES_MODAL, {
			data: { unique },
		});
	}
}
