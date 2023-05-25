import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_IMPORT_DICTIONARY_MODAL } from '@umbraco-cms/backoffice/modal';

import './import-dictionary-modal.element.js';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		const modalHandler = this.#modalContext?.open(UMB_IMPORT_DICTIONARY_MODAL, { unique: this.unique });

		// TODO: get type from modal result
		const { temporaryFileId, parentId } = await modalHandler.onSubmit();
		if (!temporaryFileId) return;

		const result = await this.repository?.import(temporaryFileId, parentId);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
