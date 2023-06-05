import { UmbDictionaryRepository } from '../../repository/dictionary.repository.js';
import { UUITextStyles } from '@umbraco-cms/backoffice/external/uui';
import { UmbSectionSidebarContext, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/section';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import {
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
	UMB_CREATE_DICTIONARY_MODAL,
} from '@umbraco-cms/backoffice/modal';

// TODO: temp import
import './create-dictionary-modal-layout.element.js';

export default class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalContext?: UmbModalManagerContext;

	#sectionSidebarContext!: UmbSectionSidebarContext;

	constructor(host: UmbControllerHostElement, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_MANAGER_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		new UmbContextConsumerController(this.host, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;
		if (!this.repository) return;

		// TODO: how can we get the current entity detail in the modal? Passing the observable
		// feels a bit hacky. Works, but hacky.
		const modalHandler = this.#modalContext?.open(UMB_CREATE_DICTIONARY_MODAL, {
			unique: this.unique,
			parentName: this.#sectionSidebarContext.headline,
		});

		// TODO: get type from modal result
		const { name } = await modalHandler.onSubmit();
		if (!name) return;

		const { data } = await this.repository.createScaffold(this.unique, name);

		// TODO => get location header to route to new item
		console.log(data);
	}
}
