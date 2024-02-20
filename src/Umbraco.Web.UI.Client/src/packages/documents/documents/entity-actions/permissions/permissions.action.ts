import type { UmbDocumentDetailRepository } from '../../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_PERMISSIONS_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPermissionsEntityAction extends UmbEntityActionBase<UmbDocumentDetailRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;
		if (!this.#modalManagerContext) return;

		this.#modalManagerContext.open(UMB_PERMISSIONS_MODAL, {
			data: {
				unique: this.unique,
				entityType: this.entityType,
			},
		});
	}
}
