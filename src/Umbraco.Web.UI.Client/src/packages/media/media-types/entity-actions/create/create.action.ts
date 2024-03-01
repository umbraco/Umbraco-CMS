import type { UmbMediaTypeDetailRepository } from '../../repository/detail/media-type-detail.repository.js';
import { UMB_MEDIA_TYPE_CREATE_OPTIONS_MODAL } from './modal/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export class UmbCreateMediaTypeEntityAction extends UmbEntityActionBase<UmbMediaTypeDetailRepository> {
	#modalManagerContext?: UmbModalManagerContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	async execute() {
		if (!this.#modalManagerContext) throw new Error('Modal manager context is not available');
		if (!this.repository) throw new Error('Repository is not available');

		this.#modalManagerContext?.open(UMB_MEDIA_TYPE_CREATE_OPTIONS_MODAL, {
			data: {
				parent: {
					unique: this.unique,
					entityType: this.entityType,
				},
			},
		});
	}
}
