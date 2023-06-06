import type { UmbDocumentRepository } from '../../repository/document.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_ALLOWED_DOCUMENT_TYPES_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';

export class UmbCreateDocumentEntityAction extends UmbEntityActionBase<UmbDocumentRepository> {
	#modalContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (this.unique) {
			await this._executeWithParent();
		} else {
			await this._executeAtRoot();
		}
	}

	private async _executeWithParent() {
		// TODO: what to do if modal service is not available?
		if (!this.repository) return;

		const { data } = await this.repository.requestById(this.unique);

		if (data && data.contentTypeId) {
			// TODO: We need to get the APP language context, use its VariantId to retrieve the right variant. The variant of which we get the name from.
			this._openModal(data.contentTypeId, data.variants?.[0]?.name);
		}
	}

	private async _executeAtRoot() {
		this._openModal(null);
	}

	private async _openModal(parentId: string | null, parentName?: string) {
		if (!this.#modalContext) return;
		const modalContext = this.#modalContext.open(UMB_ALLOWED_DOCUMENT_TYPES_MODAL, {
			parentId: parentId,
			parentName: parentName,
		});

		const { documentTypeKey } = await modalContext.onSubmit();

		// TODO: how do we want to generate these urls?
		history.pushState(
			null,
			'',
			`section/content/workspace/document/create/${this.unique ?? 'null'}/${documentTypeKey}`
		);
	}
}
