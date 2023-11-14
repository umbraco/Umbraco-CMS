import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_EXPORT_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';

export default class UmbExportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
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

		const modalContext = this.#modalContext?.open(UMB_EXPORT_DICTIONARY_MODAL, { unique: this.unique });

		// TODO: get type from modal result
		const { includeChildren } = await modalContext.onSubmit();
		if (includeChildren === undefined) return;

		// Export the file
		const result = await this.repository?.export(this.unique, includeChildren);

		const blobContent = await result?.data;
		if (!blobContent) return;

		const blob = new Blob([blobContent], { type: 'text/plain' });
		const a = document.createElement('a');

		const url = window.URL.createObjectURL(blob);
		a.href = url;
		a.download = `${this.unique}.udt`;
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);
		window.URL.revokeObjectURL(url);
	}
}
