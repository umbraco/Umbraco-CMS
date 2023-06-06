import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_EXPORT_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';

import './export-dictionary-modal.element.js';

export default class UmbExportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_EXPORT_DICTIONARY_MODAL, { unique: this.unique });

		// TODO: get type from modal result
		const { includeChildren } = await modalContext.onSubmit();
		if (includeChildren === undefined) return;

		const result = await this.repository?.export(this.unique, includeChildren);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
