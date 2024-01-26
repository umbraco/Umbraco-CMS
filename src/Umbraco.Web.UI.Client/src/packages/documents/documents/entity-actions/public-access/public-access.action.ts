import type { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UMB_PUBLIC_ACCESS_MODAL } from './modal/public-access-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_MODAL_MANAGER_CONTEXT, type UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';

export class UmbDocumentPublicAccessEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);
		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance as UmbModalManagerContext;
		});
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		await this.repository?.setPublicAccess();
		this._openModal(this.unique);
	}

	private async _openModal(unique: string) {
		this.#modalContext?.open(UMB_PUBLIC_ACCESS_MODAL, {
			data: { unique },
		});
	}
}
