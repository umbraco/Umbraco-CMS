import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UMB_MODAL_MANAGER_CONTEXT, type UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentDetailRepository } from '@umbraco-cms/backoffice/document';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<UmbDocumentDetailRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance as UmbModalManagerContext;
		});
	}

	async execute() {
		this.#modalContext?.open(UMB_PUBLIC_ACCESS_MODAL, { data: { unique: this.unique } });
	}
}
