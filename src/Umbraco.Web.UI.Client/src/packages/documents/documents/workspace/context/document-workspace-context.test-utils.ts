import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbModalManagerContext } from '@umbraco-cms/backoffice/modal';
import { UmbCurrentUserContext } from '@umbraco-cms/backoffice/current-user';
import { of } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbDocumentDetailStore } from '../../repository/detail/document-detail.store.js';
import { manifests as documentDetailRepositoryManifests } from '../../repository/detail/manifests.js';
import { UmbDocumentTypeDetailStore } from '../../../document-types/repository/detail/document-type-detail.store.js';
import { UmbDataTypeDetailStore } from '../../../../data-type/repository/detail/data-type-detail.store.js';
import { UmbDataTypeItemStore } from '../../../../data-type/repository/item/data-type-item.store.js';
import { manifests as userPermissionConditionManifests } from '../../user-permissions/document/conditions/manifests.js';
import { manifests as dataTypeItemManifests } from '../../../../data-type/repository/item/manifests.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_CREATE,
	UMB_USER_PERMISSION_DOCUMENT_UPDATE,
} from '../../user-permissions/document/constants.js';

export const TEST_MANIFESTS = [
	...documentDetailRepositoryManifests,
	...userPermissionConditionManifests,
	...dataTypeItemManifests,
];

// Grants full document create/update permissions so #enforceUserPermission resolves to permitted
// and the variant picker receives a non-empty pre-selection without needing auth infrastructure.
class UmbMockCurrentUserContext extends UmbCurrentUserContext {
	override readonly currentUser = of({
		unique: 'mock-user',
		name: 'Mock User',
		userName: 'mock@test.com',
		email: 'mock@test.com',
		languageIsoCode: 'en-US',
		avatarUrls: [],
		allowedSections: [],
		hasAccessToAllLanguages: true,
		hasAccessToSensitiveData: false,
		hasDocumentRootAccess: true,
		hasMediaRootAccess: true,
		isAdmin: false,
		languages: [],
		documentStartNodeUniques: [],
		mediaStartNodeUniques: [],
		permissions: [],
		fallbackPermissions: [UMB_USER_PERMISSION_DOCUMENT_CREATE, UMB_USER_PERMISSION_DOCUMENT_UPDATE],
		userGroupUniques: [],
	});
}

// Immediately submits every modal with its initial value so requestSave() works without UI infrastructure.
class UmbMockModalManagerContext extends UmbModalManagerContext {
	override open(...args: Parameters<UmbModalManagerContext['open']>) {
		const modalContext = super.open(...args);
		modalContext.submit();
		return modalContext;
	}
}

@customElement('umb-test-document-workspace-host')
export class UmbTestDocumentWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	constructor() {
		super();
		new UmbDocumentDetailStore(this);
		new UmbDocumentTypeDetailStore(this);
		new UmbDataTypeDetailStore(this);
		new UmbDataTypeItemStore(this);
		new UmbActionEventContext(this);
		new UmbMockModalManagerContext(this);
		new UmbMockCurrentUserContext(this);
	}
}
