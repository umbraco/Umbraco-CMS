import { UmbDataTypeDetailRepository } from '../../repository/detail/data-type-detail.repository.js';
import { UMB_DATA_TYPE_CREATE_OPTIONS_MODAL } from './modal/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbModalManagerContext, UMB_MODAL_MANAGER_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';

export class UmbCreateDataTypeEntityAction extends UmbEntityActionBase<UmbDataTypeDetailRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		this.#modalManagerContext?.open(UMB_DATA_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parentUnique: this.unique,
			},
		});
	}
}
