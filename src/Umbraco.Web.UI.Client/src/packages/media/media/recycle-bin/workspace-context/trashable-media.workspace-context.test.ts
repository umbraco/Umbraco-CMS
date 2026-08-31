import { UmbTrashableMediaWorkspaceContext } from './trashable-media.workspace-context.js';
import { UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_MEDIA_SECTION_PATH } from '../../../media-section/paths.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT,
	UmbEntityTrashedEvent,
	type UmbTrashableEntityWorkspaceContext,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-test-media-recycle-bin-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// A minimal `UmbTrashableEntityWorkspaceContext` stand-in for a real UmbMediaWorkspaceContext — the base class's own
// behaviour (readonly guard, reload, event matching) is already covered by
// trashable-entity-workspace-context-base.test.ts; this only needs to satisfy the token's discriminator and report its
// configured unique/entityType.
class FakeMediaWorkspaceContext implements UmbTrashableEntityWorkspaceContext {
	#host: UmbControllerHost;
	readonly workspaceAlias = 'Umb.Test.MediaWorkspace';
	readonly unique = new UmbObjectState('media-unique').asObservable();
	readonly isTrashed = new UmbBooleanState(undefined).asObservable();
	readonly isNew = new UmbBooleanState(undefined).asObservable();
	modalContext: unknown;
	readonly readOnlyGuard = { addRule: () => {}, removeRule: () => {} } as unknown as UmbReadOnlyVariantGuardManager;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	getUnique() {
		return 'media-unique';
	}

	getEntityType() {
		return 'media';
	}

	async reload() {}

	destroy() {}
}

class FakeMediaRecycleBinRepository {
	static originalParent: { unique: string } | null = null;
	destroy() {}
	async requestOriginalParent() {
		return { data: FakeMediaRecycleBinRepository.originalParent };
	}
}

const repositoryManifest: ManifestApi = {
	type: 'repository',
	alias: UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS,
	name: 'Fake Media Recycle Bin Repository',
	api: FakeMediaRecycleBinRepository,
};

describe('UmbTrashableMediaWorkspaceContext', () => {
	let host: UmbTestHostElement;
	let actionEventContext: UmbActionEventContext;
	let pushStateCalls: Array<{ url: string }>;
	let replaceStateCalls: Array<{ url: string }>;
	const originalPushState = window.history.pushState;
	const originalReplaceState = window.history.replaceState;

	before(() => {
		umbExtensionsRegistry.register(repositoryManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(UMB_MEDIA_RECYCLE_BIN_REPOSITORY_ALIAS);
	});

	beforeEach(async () => {
		FakeMediaRecycleBinRepository.originalParent = null;

		pushStateCalls = [];
		replaceStateCalls = [];
		window.history.pushState = (_data: unknown, _unused: string, url?: string | URL | null) => {
			pushStateCalls.push({ url: String(url) });
		};
		window.history.replaceState = (_data: unknown, _unused: string, url?: string | URL | null) => {
			replaceStateCalls.push({ url: String(url) });
		};

		host = new UmbTestHostElement();
		document.body.appendChild(host);

		actionEventContext = new UmbActionEventContext(host);
		const workspaceContext = new FakeMediaWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);

		new UmbTrashableMediaWorkspaceContext(host);
		await aTimeout(0);
	});

	afterEach(() => {
		window.history.pushState = originalPushState;
		window.history.replaceState = originalReplaceState;
		document.body.removeChild(host);
	});

	it('redirects to the parent media edit path via UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN', async () => {
		FakeMediaRecycleBinRepository.originalParent = { unique: 'parent-media-unique' };

		actionEventContext.dispatchEvent(new UmbEntityTrashedEvent({ unique: 'media-unique', entityType: 'media' }));
		await aTimeout(50);

		expect(replaceStateCalls).to.have.lengthOf(1);
		expect(replaceStateCalls[0].url).to.equal(
			UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: 'parent-media-unique' }),
		);
		expect(pushStateCalls).to.have.lengthOf(0);
	});

	it('redirects to the media section path when the trashed media item had no parent', async () => {
		FakeMediaRecycleBinRepository.originalParent = null;

		actionEventContext.dispatchEvent(new UmbEntityTrashedEvent({ unique: 'media-unique', entityType: 'media' }));
		await aTimeout(50);

		expect(pushStateCalls).to.have.lengthOf(1);
		expect(pushStateCalls[0].url).to.equal(UMB_MEDIA_SECTION_PATH);
		expect(replaceStateCalls).to.have.lengthOf(0);
	});

	it('does not redirect for an unrelated entity type using the same repository alias', async () => {
		FakeMediaRecycleBinRepository.originalParent = { unique: 'parent-media-unique' };

		actionEventContext.dispatchEvent(
			new UmbEntityTrashedEvent({ unique: 'media-unique', entityType: 'some-other-entity-type' }),
		);
		await aTimeout(50);

		expect(pushStateCalls).to.have.lengthOf(0);
		expect(replaceStateCalls).to.have.lengthOf(0);
	});
});
