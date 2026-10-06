import { UmbDocumentDetailStore } from '../../repository/detail/document-detail.store.js';
import { manifests as documentDetailRepositoryManifests } from '../../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../../document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../../../data-type/repository/detail/data-type-detail.store.js';
import { UmbDataTypeItemStore } from '../../../../data-type/repository/item/data-type-item.store.js';
import { manifests as userPermissionConditionManifests } from '../../user-permissions/document/conditions/manifests.js';
import { manifests as dataTypeItemManifests } from '../../../../data-type/repository/item/manifests.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import type { UmbModalContextClassArgs, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UMB_APP_LANGUAGE_CONTEXT, UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

export const TEST_MANIFESTS = [
	...documentDetailRepositoryManifests,
	...userPermissionConditionManifests,
	...dataTypeItemManifests,
];

// Immediately submits every modal with its initial value so requestSave() works without UI infrastructure.
class UmbMockModalManagerContext extends UmbModalManagerContext {
	override open<
		ModalData extends { [key: string]: any } = { [key: string]: any },
		ModalValue = unknown,
		ModalAliasTypeAsToken extends UmbModalToken = UmbModalToken<ModalData, ModalValue>,
	>(
		host: UmbControllerHost,
		modalAlias: UmbModalToken<ModalData, ModalValue> | string,
		args?: UmbModalContextClassArgs<ModalAliasTypeAsToken>,
	) {
		const modalContext = super.open<ModalData, ModalValue, ModalAliasTypeAsToken>(host, modalAlias, args);
		modalContext.submit();
		return modalContext;
	}
}

// The workspace observes UMB_APP_LANGUAGE_CONTEXT for languages. The real context is auth-gated and
// self-loading, so tests provide this minimal stand-in and fill it from the mock server instead.
class UmbTestAppLanguageContext {
	#host: UmbControllerHost;
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	readonly languages = this.#languages.asObservable();

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	async load() {
		const { data } = await new UmbLanguageCollectionRepository(this.#host).requestAllItems();
		this.#languages.setValue(data?.items ?? []);
	}

	setLanguages(languages: Array<UmbLanguageDetailModel>) {
		this.#languages.setValue(languages);
	}
}

@customElement('umb-test-document-workspace-host')
export class UmbTestDocumentWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	#currentUserContext = new UmbCurrentUserContext(this);
	#appLanguageContext = new UmbTestAppLanguageContext(this);

	constructor() {
		super();
		new UmbDocumentDetailStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDataTypeDetailStore(this);
		new UmbDataTypeItemStore(this);
		new UmbActionEventContext(this);
		new UmbMockModalManagerContext(this);
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
		new UmbContextProviderController(this, UMB_APP_LANGUAGE_CONTEXT, this.#appLanguageContext as never);
	}

	public async init() {
		await this.#currentUserContext.load();
		await this.#appLanguageContext.load();
	}

	public setAppLanguages(languages: Array<UmbLanguageDetailModel>) {
		this.#appLanguageContext.setLanguages(languages);
	}
}
