import { UmbDocumentRepository } from '../../repository/document.repository';
import type { UmbCreateDocumentModalResultData } from './create-document-modal-layout.element';
import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

// TODO: temp import
import './create-document-modal-layout.element.ts';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
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

		const modalHandler = this.#modalContext?.open('umb-create-document-modal-layout', {
			type: 'sidebar',
			data: { unique: this.unique },
		});

		// TODO: get type from modal result
		const { documentType }: UmbCreateDocumentModalResultData = await modalHandler.onSubmit();
		alert('create document with document type: ' + documentType);
	}
}
