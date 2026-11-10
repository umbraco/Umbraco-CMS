import { UmbElementDetailStore } from '../repository/detail/element-detail.store.js';
import { manifests as elementDetailRepositoryManifests } from '../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../documents/document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../data-type/repository/detail/data-type-detail.store.js';
import { UmbDataTypeItemStore } from '../../data-type/repository/item/data-type-item.store.js';
import { manifests as userPermissionConditionManifests } from '../user-permissions/conditions/manifests.js';
import { manifests as dataTypeItemManifests } from '../../data-type/repository/item/manifests.js';
import { manifests as elementReferenceRepositoryManifests } from '../reference/repository/manifests.js';
import { UmbCurrentUserContext, UmbCurrentUserStore } from '@umbraco-cms/backoffice/current-user';
import type { UmbModalContextClassArgs, UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

export const TEST_MANIFESTS = [
	...elementDetailRepositoryManifests,
	...userPermissionConditionManifests,
	...dataTypeItemManifests,
	...elementReferenceRepositoryManifests,
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

// Workspaces derive their variant options from the app languages, so they never resolve without this context.
class UmbMockAppLanguageContext extends UmbContextBase {
	readonly #languages = new UmbArrayState<UmbLanguageDetailModel>(
		[
			{
				entityType: 'language',
				unique: 'en-US',
				name: 'English (United States)',
				isDefault: true,
				isMandatory: false,
				fallbackIsoCode: null,
			},
		],
		(x) => x.unique,
	);
	readonly languages = this.#languages.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_APP_LANGUAGE_CONTEXT);
	}
}

@customElement('umb-test-element-workspace-host')
export class UmbTestElementWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	readonly #currentUserContext = new UmbCurrentUserContext(this);

	constructor() {
		super();
		new UmbElementDetailStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDataTypeDetailStore(this);
		new UmbDataTypeItemStore(this);
		new UmbActionEventContext(this);
		new UmbMockModalManagerContext(this);
		new UmbNotificationContext(this);
		new UmbCurrentUserStore(this);
		new UmbMockAppLanguageContext(this);
	}

	public async init() {
		await this.#currentUserContext.load();
	}
}
