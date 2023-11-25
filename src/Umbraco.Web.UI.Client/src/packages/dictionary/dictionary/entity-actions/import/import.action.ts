import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_IMPORT_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UmbTextStyles];

	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_IMPORT_DICTIONARY_MODAL, { unique: this.unique });

		const { parentId, temporaryFileId } = await modalContext.onSubmit();

		await this.repository?.import(temporaryFileId, parentId);
	}
}
