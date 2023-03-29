import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN, UMB_CONFIRM_MODAL } from '@umbraco-cms/backoffice/modal';

export class UmbTrashEntityAction<
	T extends { trash(unique: Array<string>): Promise<void>; requestTreeItems(uniques: Array<string>): any }
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

		const { data } = await this.repository.requestTreeItems([this.unique]);

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
