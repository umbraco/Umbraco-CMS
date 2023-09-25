import { UmbPartialViewsRepository } from '../../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_MANAGER_CONTEXT_TOKEN, UmbModalManagerContext, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { SnippetItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbCreateFromSnippetPartialViewModalData {
	snippets: SnippetItemResponseModel[];
}

export type UmbConfirmModalValue = undefined;

export const UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL = new UmbModalToken<UmbCreateFromSnippetPartialViewModalData, string>(
	'Umb.Modal.CreateFromSnippetPartialView',
	{
		type: 'sidebar',
	},
);

export class UmbCreateFromSnippetPartialViewAction<
	T extends { copy(): Promise<void> },
> extends UmbEntityActionBase<UmbPartialViewsRepository> {
	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
	}

	#modalContext?: UmbModalManagerContext;

	async execute() {
		const snippets = (await this.repository?.getSnippets({}))?.data?.items ?? [];

		const modalContext = this.#modalContext?.open(UMB_PARTIAL_VIEW_FROM_SNIPPET_MODAL, {
			snippets,
		});

		await modalContext?.onSubmit().then((snippetName) => {
			history.pushState(
				null,
				'',
				`section/settings/workspace/partial-view/create/${this.unique ?? 'null'}/${snippetName}`,
			);
		});
	}
}
