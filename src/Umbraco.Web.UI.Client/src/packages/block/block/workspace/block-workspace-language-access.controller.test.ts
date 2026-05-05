import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import { UmbBlockLanguageAccessWorkspaceController } from './block-workspace-language-access.controller.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, UmbBasicState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

class UmbBlockWorkspaceContextStub extends UmbContextBase {
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;
	readonly #variantId = new UmbBasicState<UmbVariantId | undefined>(undefined);
	readonly variantId = this.#variantId.asObservable();
	public readonly readOnlyGuard = new UmbReadOnlyVariantGuardManager(this);
	public readonly content = { readOnlyGuard: new UmbReadOnlyVariantGuardManager(this) };
	public readonly settings = { readOnlyGuard: new UmbReadOnlyVariantGuardManager(this) };

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_WORKSPACE_CONTEXT.toString());
	}

	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
	}
}

class UmbBlockManagerContextStub extends UmbContextBase {
	readonly #permitted = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly readOnlyState = { permitted: this.#permitted.asObservable() };

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT.toString());
	}

	setPermitted(value: boolean) {
		this.#permitted.setValue(value);
	}
}

class UmbCurrentUserContextStub extends UmbContextBase {
	readonly #languages = new UmbArrayState<string>([], (x) => x);
	public readonly languages = this.#languages.asObservable();
	readonly #hasAccessToAllLanguages = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly hasAccessToAllLanguages = this.#hasAccessToAllLanguages.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT.toString());
	}

	setLanguages(languages: Array<string>) {
		this.#languages.setValue(languages);
	}

	setHasAccessToAllLanguages(value: boolean) {
		this.#hasAccessToAllLanguages.setValue(value);
	}
}

@customElement('umb-test-block-language-access-host')
class UmbTestBlockLanguageAccessHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbBlockWorkspaceContextStub;
	blockManagerContext!: UmbBlockManagerContextStub;
	currentUserContext!: UmbCurrentUserContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbBlockWorkspaceContextStub(this);
		this.blockManagerContext = new UmbBlockManagerContextStub(this);
		this.currentUserContext = new UmbCurrentUserContextStub(this);
	}
}

const enUS = UmbVariantId.Create({ culture: 'en-US', segment: null });
const daDK = UmbVariantId.Create({ culture: 'da-DK', segment: null });

async function flushMicrotasks() {
	// Two ticks: one for context-consumer resolution, one for the inner observe to fire.
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((r) => setTimeout(r, 0));
}

describe('UmbBlockLanguageAccessWorkspaceController', () => {
	let host: UmbTestBlockLanguageAccessHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-block-language-access-host></umb-test-block-language-access-host>`);
	});

	afterEach(() => {
		host.remove();
	});

	function expectReadOnly(variantId: UmbVariantId) {
		expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(variantId), 'workspace.readOnlyGuard').to.be
			.true;
		expect(host.workspaceContext.content.readOnlyGuard.getIsPermittedForVariant(variantId), 'content.readOnlyGuard').to
			.be.true;
		expect(host.workspaceContext.settings.readOnlyGuard.getIsPermittedForVariant(variantId), 'settings.readOnlyGuard')
			.to.be.true;
	}

	function expectEditable(variantId: UmbVariantId) {
		expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(variantId), 'workspace.readOnlyGuard').to.be
			.false;
		expect(host.workspaceContext.content.readOnlyGuard.getIsPermittedForVariant(variantId), 'content.readOnlyGuard').to
			.be.false;
		expect(host.workspaceContext.settings.readOnlyGuard.getIsPermittedForVariant(variantId), 'settings.readOnlyGuard')
			.to.be.false;
	}

	describe('Invariant block — block manager state', () => {
		it('is read-only when the block manager is read-only', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectReadOnly(UmbVariantId.CreateInvariant());
		});

		it('is editable when the block manager is not read-only', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectEditable(UmbVariantId.CreateInvariant());
		});

		it('flips to editable when the block manager flips from read-only to not read-only', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());

			host.blockManagerContext.setPermitted(false);
			await flushMicrotasks();
			expectEditable(UmbVariantId.CreateInvariant());
		});

		it('flips to read-only when the block manager flips from not read-only to read-only', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectEditable(UmbVariantId.CreateInvariant());

			host.blockManagerContext.setPermitted(true);
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());
		});
	});

	describe('Variant block — language access', () => {
		it('is editable when the user has access to all languages', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectEditable(enUS);
		});

		it('is editable when the culture is in the user allowed languages', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages(['en-US']);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectEditable(enUS);
		});

		it('is read-only when the culture is not in the user allowed languages', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages(['da-DK']);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectReadOnly(enUS);
		});

		it('is read-only when the user has neither global access nor a matching language', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectReadOnly(enUS);
		});
	});

	describe('Transitions', () => {
		it('updates correctly when the variantId switches culture (en-US → da-DK)', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages(['da-DK']);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectReadOnly(enUS);

			host.workspaceContext.setVariantId(daDK);
			await flushMicrotasks();
			expectEditable(daDK);
		});

		it('drops the block-manager read-only state when switching from invariant to variant', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());

			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			expectEditable(enUS);
		});

		it('stays correct after multiple invariant ↔ variant transitions', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());

			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			expectEditable(enUS);

			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());

			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			expectEditable(enUS);

			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			await flushMicrotasks();
			expectReadOnly(UmbVariantId.CreateInvariant());
		});
	});
});
