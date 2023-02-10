import { UmbDocumentRepository } from '../../repository/document.repository';
import { UmbEntityActionBase } from '../../../../shared/entity-actions';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbModalService, UMB_MODAL_SERVICE_CONTEXT_TOKEN } from '@umbraco-cms/modal';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';

// TODO: temp import
import './create-document-modal-layout.element.ts';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalService?: UmbModalService;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_SERVICE_CONTEXT_TOKEN, (instance) => {
			this.#modalService = instance;
		});
	}

	async execute() {
		const modalHandler = this.#modalService?.open('umb-create-document-modal-layout', {
			type: 'sidebar',
			data: { unique: this.unique },
		});

		const result = await modalHandler?.onClose();
		console.log(`execute for: ${this.unique}`);
		alert('open create dialog');
	}
}
