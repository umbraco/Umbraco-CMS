import type { UmbDictionaryExportRepository } from '../../repository/index.js';
import { UMB_EXPORT_DICTIONARY_MODAL } from './export-dictionary-modal.token.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export default class UmbExportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryExportRepository> {
	static styles = [UmbTextStyles];

	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_EXPORT_DICTIONARY_MODAL, { data: { unique: this.unique } });

		const { includeChildren } = await modalContext.onSubmit();

		// Export the file
		const result = await this.repository?.requestExport(this.unique, includeChildren);
		const blobContent = await result?.data;

		if (!blobContent) return;
		const blob = new Blob([blobContent], { type: 'text/plain' });
		const a = document.createElement('a');
		const url = window.URL.createObjectURL(blob);

		// Download
		a.href = url;
		a.download = `${this.unique}.udt`;
		document.body.appendChild(a);
		a.click();
		document.body.removeChild(a);

		// Clean up
		window.URL.revokeObjectURL(url);
	}
}
