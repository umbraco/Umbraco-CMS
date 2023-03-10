import { UUITextStyles } from '@umbraco-ui/uui-css';
import { UmbDictionaryRepository } from '../../repository/dictionary.repository';
import {
	UmbSectionSidebarContext,
	UMB_SECTION_SIDEBAR_CONTEXT_TOKEN,
} from '../../../../../backoffice/shared/components/section/section-sidebar/section-sidebar.context';
import { UMB_CREATE_DICTIONARY_MODAL_TOKEN } from '.';
import { UmbEntityActionBase } from '@umbraco-cms/entity-action';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { UmbContextConsumerController } from '@umbraco-cms/context-api';
import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/modal';

// TODO: temp import
import './create-dictionary-modal-layout.element';

export default class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryRepository> {
	static styles = [UUITextStyles];

	#modalContext?: UmbModalContext;

	#sectionSidebarContext!: UmbSectionSidebarContext;

	constructor(host: UmbControllerHostInterface, repositoryAlias: string, unique: string) {
		super(host, repositoryAlias, unique);

		new UmbContextConsumerController(this.host, UMB_MODAL_CONTEXT_TOKEN, (instance) => {
			this.#modalContext = instance;
		});

		new UmbContextConsumerController(this.host, UMB_SECTION_SIDEBAR_CONTEXT_TOKEN, (instance) => {
			this.#sectionSidebarContext = instance;
		});
	}

	async execute() {
		// TODO: what to do if modal service is not available?
		if (!this.#modalContext) return;

		// TODO: how can we get the current entity detail in the modal? Passing the observable
		// feels a bit hacky. Works, but hacky.
		const modalHandler = this.#modalContext?.open(UMB_CREATE_DICTIONARY_MODAL_TOKEN, {
			unique: this.unique,
			parentName: this.#sectionSidebarContext.headline,
		});

		// TODO: get type from modal result
		const { name } = await modalHandler.onSubmit();
		if (!name) return;

		const result = await this.repository?.create({
			$type: '',
			name,
			parentKey: this.unique,
			translations: [],
			key: '',
		});

		// TODO => get location header to route to new item
		console.log(result);
	}
}
