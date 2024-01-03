import { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CULTURE_AND_HOSTNAMES_MODAL as UMB_CULTURE_AND_HOSTNAMES_MODAL,
} from '@umbraco-cms/backoffice/modal';

export class UmbDocumentCultureAndHostnamesEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		console.log(`execute for: ${this.unique}`);
		//await this.repository?.setCultureAndHostnames();

		if (!this.repository) return;
		this._openModal(this.unique || null);
	}

	private async _openModal(unique: string | null) {
		if (!this.#modalContext) return;
		const modalContext = this.#modalContext.open(UMB_CULTURE_AND_HOSTNAMES_MODAL, {
			data: { unique },
		});

		const { data } = await modalContext.onSubmit();
		console.table(data);
	}
}
