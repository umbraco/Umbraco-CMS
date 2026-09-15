import { UmbTrashableDocumentWorkspaceContext } from './trashable-document.workspace-context.js';
import { UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN } from '../../paths.js';
import { UMB_DOCUMENTS_SECTION_PATH } from '../../../section/paths.js';
import { aTimeout, expect } from '@open-wc/testing';
import { UmbActionEventContext } from '@umbraco-cms/backoffice/action';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbParentEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbBooleanState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT,
	UmbEntityTrashedEvent,
	type UmbTrashableEntityWorkspaceContext,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';

@customElement('umb-test-document-recycle-bin-host')
class UmbTestHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Only needs to satisfy the token's discriminator and report its configured unique/entityType — the base class's
// own trash/restore/readonly behaviour is tested separately, not here.
class FakeDocumentWorkspaceContext implements UmbTrashableEntityWorkspaceContext {
	#host: UmbControllerHost;
	readonly workspaceAlias = 'Umb.Test.DocumentWorkspace';
	readonly unique = new UmbObjectState('document-unique').asObservable();
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
		return 'document-unique';
	}

	getEntityType() {
		return 'document';
	}

	resetData() {}

	async reload() {}

	destroy() {}
}

describe('UmbTrashableDocumentWorkspaceContext', () => {
	let host: UmbTestHostElement;
	let actionEventContext: UmbActionEventContext;
	let parentEntityContext: UmbParentEntityContext;
	let pushStateCalls: Array<{ url: string }>;
	let replaceStateCalls: Array<{ url: string }>;
	const originalPushState = window.history.pushState;
	const originalReplaceState = window.history.replaceState;

	beforeEach(async () => {
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
		const workspaceContext = new FakeDocumentWorkspaceContext(host);
		new UmbContextProviderController(host, UMB_TRASHABLE_ENTITY_WORKSPACE_CONTEXT, workspaceContext as never);
		parentEntityContext = new UmbParentEntityContext(host);

		new UmbTrashableDocumentWorkspaceContext(host);
		await aTimeout(0);
	});

	afterEach(() => {
		window.history.pushState = originalPushState;
		window.history.replaceState = originalReplaceState;
		document.body.removeChild(host);
	});

	it('redirects to the parent document edit path via UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN', () => {
		parentEntityContext.setParent({ unique: 'parent-document-unique', entityType: 'document' });

		actionEventContext.dispatchEvent(new UmbEntityTrashedEvent({ unique: 'document-unique', entityType: 'document' }));

		expect(pushStateCalls).to.have.lengthOf(1);
		expect(pushStateCalls[0].url).to.equal(
			UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: 'parent-document-unique' }),
		);
		expect(replaceStateCalls).to.have.lengthOf(0);
	});

	it('redirects to the documents section path when the trashed document had no parent', () => {
		actionEventContext.dispatchEvent(new UmbEntityTrashedEvent({ unique: 'document-unique', entityType: 'document' }));

		expect(pushStateCalls).to.have.lengthOf(1);
		expect(pushStateCalls[0].url).to.equal(UMB_DOCUMENTS_SECTION_PATH);
		expect(replaceStateCalls).to.have.lengthOf(0);
	});

	it('does not redirect for an unrelated entity type', () => {
		parentEntityContext.setParent({ unique: 'parent-document-unique', entityType: 'document' });

		actionEventContext.dispatchEvent(
			new UmbEntityTrashedEvent({ unique: 'document-unique', entityType: 'some-other-entity-type' }),
		);

		expect(pushStateCalls).to.have.lengthOf(0);
		expect(replaceStateCalls).to.have.lengthOf(0);
	});
});
