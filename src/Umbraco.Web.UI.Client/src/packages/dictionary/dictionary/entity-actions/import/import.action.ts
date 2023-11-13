import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UmbTextStyles } from "@umbraco-cms/backoffice/style";
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
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_IMPORT_DICTIONARY_MODAL, { unique: this.unique });

		// TODO: get type from modal result
		const { temporaryFileId, parentId } = await modalContext.onSubmit();
		if (!temporaryFileId) return;

		const result = await this.repository?.import(temporaryFileId, parentId);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
