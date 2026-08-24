import { UmbDocumentDetailStore } from '../../repository/detail/document-detail.store.js';
import { manifests as documentDetailRepositoryManifests } from '../../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../../document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../../../data-type/repository/detail/data-type-detail.store.js';
import { UmbDataTypeItemStore } from '../../../../data-type/repository/item/data-type-item.store.js';
import { manifests as userPermissionConditionManifests } from '../../user-permissions/document/conditions/manifests.js';
import { manifests as dataTypeItemManifests } from '../../../../data-type/repository/item/manifests.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import { UMB_APP_LANGUAGE_CONTEXT, UMB_LANGUAGE_ENTITY_TYPE } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbModalContextClassArgs, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

// Mirrors the languages of the mock data sets these tests run against.
const TEST_LANGUAGES: Array<UmbLanguageDetailModel> = [
	{
		entityType: UMB_LANGUAGE_ENTITY_TYPE,
		unique: 'en-US',
		name: 'English',
		isDefault: true,
		isMandatory: true,
		fallbackIsoCode: null,
	},
	{
		entityType: UMB_LANGUAGE_ENTITY_TYPE,
		unique: 'da',
		name: 'Danish',
		isDefault: false,
		isMandatory: false,
		fallbackIsoCode: 'en-US',
	},
];

// The workspace derives its variant options from the app languages, so a culture-varying document has no
// cultures to expand to unless they are provided. Serving them from the host keeps that independent of
// whichever mock data set happens to be loaded when the test runs.
class UmbTestAppLanguageContext extends UmbControllerBase {
	#languages = new UmbArrayState<UmbLanguageDetailModel>(TEST_LANGUAGES, (x) => x.unique);
	readonly languages = this.#languages.asObservable();
	readonly cultures = this.#languages.asObservablePart((x) => x.map((y) => y.unique));
	readonly appDefaultLanguage = this.#languages.asObservablePart((x) => x.find((y) => y.isDefault));
	readonly moreThanOneLanguage = this.#languages.asObservablePart((x) => x.length > 1);

	#appLanguage = new UmbObjectState<UmbLanguageDetailModel | undefined>(TEST_LANGUAGES[0]);
	readonly appLanguage = this.#appLanguage.asObservable();
	readonly appLanguageCulture = this.#appLanguage.asObservablePart((x) => x?.unique);

	getAppCulture() {
		return this.#appLanguage.getValue()?.unique;
	}
}

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

@customElement('umb-test-document-workspace-host')
export class UmbTestDocumentWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	#currentUserContext = new UmbCurrentUserContext(this);

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
		new UmbContextProviderController(
			this,
			UMB_APP_LANGUAGE_CONTEXT,
			new UmbTestAppLanguageContext(this) as unknown as typeof UMB_APP_LANGUAGE_CONTEXT.TYPE,
		);
	}

	public async init() {
		await this.#currentUserContext.load();
	}
}
