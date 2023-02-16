import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbEntityActionBase } from '../../../../shared/entity-actions';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository';
import type { UmbImportDictionaryModalResultData } from './import-dictionary-modal-layout.element';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

import './import-dictionary-modal-layout.element';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalService) return;

		const modalHandler = this.#modalService?.open('umb-import-dictionary-modal-layout', {
			type: 'sidebar',
			data: { unique: this.unique },
		});

		// TODO: get type from modal result
		const { fileName, parentKey }: UmbImportDictionaryModalResultData = await modalHandler.onClose();
		if (!fileName) return;

		const result = await this.repository?.import(fileName, parentKey);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
