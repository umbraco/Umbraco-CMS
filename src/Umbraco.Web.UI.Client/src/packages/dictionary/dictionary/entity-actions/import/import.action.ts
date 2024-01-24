import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_IMPORT_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';
import { UMB_DICTIONARY_TREE_STORE_CONTEXT, UmbDictionaryTreeStore } from '@umbraco-cms/backoffice/dictionary';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UmbTextStyles];

	#modalContext?: UmbModalManagerContext;
	#treeStore?: UmbDictionaryTreeStore;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});
		this.consumeContext(UMB_DICTIONARY_TREE_STORE_CONTEXT, (instance) => {
			this.#treeStore = instance;
		});
	}

	async execute() {
		if (!this.#modalContext) return;

		const modalContext = this.#modalContext?.open(UMB_IMPORT_DICTIONARY_MODAL, { data: { unique: this.unique } });

		const { entityItems, parentId } = await modalContext.onSubmit();

		if (!entityItems?.length) return;

		this.#treeStore?.appendItems(entityItems);

		if (parentId) this.#treeStore?.updateItem(parentId, { hasChildren: true });
	}
}
