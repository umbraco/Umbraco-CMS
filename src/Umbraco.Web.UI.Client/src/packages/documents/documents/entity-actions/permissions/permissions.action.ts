import { type UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UMB_PERMISSIONS_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPermissionsEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext.open(UMB_PERMISSIONS_MODAL, {
			unique: this.unique,
			entityType: 'document',
		});
	}
}
