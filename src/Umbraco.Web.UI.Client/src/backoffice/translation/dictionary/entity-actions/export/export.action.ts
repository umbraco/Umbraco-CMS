import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository';
import type { UmbExportDictionaryModalResultData } from './export-dictionary-modal-layout.element';
import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

import './export-dictionary-modal-layout.element';

export default class UmbExportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		const modalHandler = this.#modalContext?.open('umb-export-dictionary-modal-layout', {
			type: 'sidebar',
			data: { unique: this.unique },
		});

		// TODO: get type from modal result
		const { includeChildren }: UmbExportDictionaryModalResultData = await modalHandler.onSubmit();
		if (includeChildren === undefined) return;

		const result = await this.repository?.export(this.unique, includeChildren);

		// TODO => get location header to route to new item
		console.log(result);
	}
}
