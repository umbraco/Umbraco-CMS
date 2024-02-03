import type { UmbDictionaryImportRepository } from '../../repository/index.js';
import { UmbTextStyles } from '@umbraco-cms/backoffice/style';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UMB_MODAL_MANAGER_CONTEXT, UMB_IMPORT_DICTIONARY_MODAL } from '@umbraco-cms/backoffice/modal';
import type { UmbDictionaryTreeStore } from 'src/packages/dictionary/index.js';
import { UMB_DICTIONARY_TREE_STORE_CONTEXT } from 'src/packages/dictionary/index.js';

export default class UmbImportDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryImportRepository> {
	static styles = [UmbTextStyles];

	#modalContext?: UmbModalManagerContext;
	#treeStore?: UmbDictionaryTreeStore;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string, entityType: string) {
		super(host, repositoryAlias, unique, entityType);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
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
