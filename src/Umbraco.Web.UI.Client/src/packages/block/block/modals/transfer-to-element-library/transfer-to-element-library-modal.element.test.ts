import { useMockHandlers, resetMockHandlers } from '../../../../../../mocks/index.js';
import { UmbBlockTransferToElementLibraryModalElement } from './transfer-to-element-library-modal.element.js';
import { expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbSelectedEvent } from '@umbraco-cms/backoffice/event';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as permissionRepositoryManifests } from '../../../../elements/user-permissions/repository/manifests.js';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UserPermissionsResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UUIButtonElement } from '@umbraco-cms/backoffice/external/uui';

const { http, HttpResponse } = window.MockServiceWorker;

const CREATE = 'Umb.Element.Create';
const PUBLISH = 'Umb.Element.Publish';

class UmbCurrentUserContextStub extends UmbContextBase {
	readonly #currentUser = new UmbObjectState<{
		permissions: Array<unknown>;
		fallbackPermissions: Array<string>;
		hasElementRootAccess: boolean;
	}>({
		permissions: [],
		fallbackPermissions: [],
		hasElementRootAccess: true,
	});
	public readonly currentUser = this.#currentUser.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT.toString());
	}

	setFallbackPermissions(fallbackPermissions: Array<string>, hasElementRootAccess = true) {
		this.#currentUser.setValue({ permissions: [], fallbackPermissions, hasElementRootAccess });
	}
}

@customElement('umb-test-transfer-to-element-library-modal-host')
class UmbTestTransferToElementLibraryModalHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	currentUserContext = new UmbCurrentUserContextStub(this);
}

const useElementPermissions = (verbsByUnique: Record<string, Array<string>>) => {
	const requestedIds: Array<string> = [];
	useMockHandlers(
		http.get(umbracoPath('/user/current/permissions/element'), ({ request }) => {
			const ids = new URL(request.url).searchParams.getAll('id');
			requestedIds.push(...ids);
			const response: UserPermissionsResponseModel = {
				permissions: ids.map((id) => ({ nodeKey: id, permissions: verbsByUnique[id] ?? [] })),
			};
			return HttpResponse.json(response);
		}),
	);
	return requestedIds;
};

describe('UmbBlockTransferToElementLibraryModalElement', () => {
	let host: UmbTestTransferToElementLibraryModalHostElement;
	let modal: UmbBlockTransferToElementLibraryModalElement;

	const getTransferButton = () => modal.shadowRoot!.querySelector<UUIButtonElement>('uui-button[look="primary"]')!;

	const getLocationError = () => modal.shadowRoot!.querySelector('[slot="message"]')?.textContent ?? undefined;

	const selectLocation = async (unique: string | null) => {
		modal.shadowRoot!.querySelector('umb-tree')!.dispatchEvent(new UmbSelectedEvent(unique));
		await modal.updateComplete;
		await waitUntil(() => getTransferButton().state !== 'waiting', 'The location check did not complete');
		await modal.updateComplete;
	};

	beforeEach(async () => {
		umbExtensionsRegistry.registerMany(permissionRepositoryManifests);
		host = new UmbTestTransferToElementLibraryModalHostElement();
		document.body.appendChild(host);
		modal = new UmbBlockTransferToElementLibraryModalElement();
		modal.data = { name: 'Block' } as UmbBlockTransferToElementLibraryModalElement['data'];
		host.appendChild(modal);
		await modal.updateComplete;
	});

	afterEach(() => {
		umbExtensionsRegistry.unregisterMany(permissionRepositoryManifests.map((manifest) => manifest.alias));
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('uses the permissions the server resolves for the selected folder', async () => {
		host.currentUserContext.setFallbackPermissions([CREATE, PUBLISH]);
		useElementPermissions({ 'denied-folder': [PUBLISH], 'allowed-folder': [CREATE, PUBLISH] });

		await selectLocation('denied-folder');
		expect(getLocationError()).to.not.be.undefined;
		expect(getTransferButton().disabled).to.be.true;

		await selectLocation('allowed-folder');
		expect(getLocationError()).to.be.undefined;
		expect(getTransferButton().disabled).to.be.false;
	});

	it('uses the fallback permissions at the library root', async () => {
		const requestedIds = useElementPermissions({});

		host.currentUserContext.setFallbackPermissions([CREATE]);
		await selectLocation(null);
		expect(getLocationError()).to.not.be.undefined;

		host.currentUserContext.setFallbackPermissions([CREATE, PUBLISH]);
		await selectLocation(null);
		expect(getLocationError()).to.be.undefined;

		expect(requestedIds).to.be.empty;
	});

	it('refuses the library root when the user has no element root access', async () => {
		host.currentUserContext.setFallbackPermissions([CREATE, PUBLISH], false);
		await selectLocation(null);

		expect(getLocationError()).to.not.be.undefined;
		expect(getTransferButton().disabled).to.be.true;
	});
});
