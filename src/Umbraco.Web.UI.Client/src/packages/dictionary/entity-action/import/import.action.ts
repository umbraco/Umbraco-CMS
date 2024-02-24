import type { UmbDictionaryImportRepository } from '../../repository/index.js';
import { UMB_IMPORT_DICTIONARY_MODAL } from './import-dictionary-modal.token.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryImportRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_IMPORT_DICTIONARY_MODAL, { data: { unique: this.unique } });
		await modalContext.onSubmit();
	}
}
