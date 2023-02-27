import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbEntityActionBase } from '../../../../../../libs/entity-action';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository';
import type { UmbExportDictionaryModalResultData } from './export-dictionary-modal-layout.element';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';

import './export-dictionary-modal-layout.element';

export default class UmbExportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
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

		const modalHandler = this.#modalService?.open('umb-export-dictionary-modal-layout', {
			type: 'sidebar',
			data: { unique: this.unique },
		});

		// TODO: get type from modal result
		const { includeChildren }: UmbExportDictionaryModalResultData = await modalHandler.onClose();
		if (includeChildren === undefined) return;

		const result = await this.repository?.export(this.unique, includeChildren);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
