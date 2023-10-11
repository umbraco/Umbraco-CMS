import { type UmbEnableUserRepository } from '../../repository/enable-user.repository.js';
import { UmbUserRepository } from '../../repository/user.repository.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	type UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CONFIRM_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { type UmbItemRepository } from '@umbraco-cms/backoffice/repository';
import { type UserItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbEnableUserEntityAction<
	RepositoryType extends UmbEnableUserRepository & UmbItemRepository<UserItemResponseModel>,
> extends UmbEntityActionBase<RepositoryType> {
	#modalManager?: UmbModalManagerContext;
	#itemRepository: UmbUserRepository;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		this.#itemRepository = new UmbUserRepository(this.host);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalManager = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalManager) return;

		const { data } = await this.#itemRepository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			const modalContext = this.#modalManager.open(UMB_CONFIRM_MODAL, {
				headline: `Enable ${item.name}`,
				content: 'Are you sure you want to enable this user?',
				confirmLabel: 'Enable',
			});

			await modalContext.onSubmit();
			await this.repository?.enable([this.unique]);
		}
	}
}
