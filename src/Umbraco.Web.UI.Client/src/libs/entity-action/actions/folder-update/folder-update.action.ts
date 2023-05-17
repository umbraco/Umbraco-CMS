import { UmbEntityActionBase } from 'src/libs/entity-action';
import { UmbContextConsumerController } from 'src/libs/context-api';
import { UmbControllerHostElement } from 'src/libs/controller-api';
import { UmbModalContext, UMB_FOLDER_MODAL, UMB_MODAL_CONTEXT_TOKEN } from 'src/libs/modal';
import { UmbFolderRepository } from 'src/libs/repository';

export class UmbFolderUpdateEntityAction<
	T extends UmbFolderRepository = UmbFolderRepository
> extends UmbEntityActionBase<T> {
	#modalContext?: UmbModalContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	async execute() {
		if (!this.repository || !this.#modalContext) return;

		const modalHandler = this.#modalContext.open(UMB_FOLDER_MODAL, {
			repositoryAlias: this.repositoryAlias,
			unique: this.unique,
		});

		await modalHandler.onSubmit();
	}
}
