import { UmbEntityActionBase } from 'src/libs/entity-action';
import { UmbContextConsumerController } from 'src/libs/context-api';
import { UmbControllerHostElement } from 'src/libs/controller-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from 'src/libs/modal';
import { UmbItemRepository } from 'src/libs/repository';

export class UmbTrashEntityAction<
	T extends UmbItemRepository<any> & { trash(unique: Array<string>): Promise<void> }
> extends UmbEntityActionBase<T> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository) return;

		const { data } = await this.repository.requestItems([this.unique]);

		if (data) {
			const item = data[0];

			const modalHandler = this.#modalContext?.open(UMB_CONFIRM_MODAL, {
				headline: `Trash ${item.name}`,
				content: 'Are you sure you want to move this item to the recycle bin?',
				color: 'danger',
				confirmLabel: 'Trash',
			});

			modalHandler?.onSubmit().then(() => {
				this.repository?.trash([this.unique]);
			});
		}
	}
}
