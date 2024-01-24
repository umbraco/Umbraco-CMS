import { type UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UmbModalManagerContext,
	UMB_PERMISSIONS_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPermissionsEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;
		if (!this.#modalManagerContext) return;

		this.#modalManagerContext.open(UMB_PERMISSIONS_MODAL, {
			data: {
				unique: this.unique,
				entityType: 'document',
			},
		});
	}
}
