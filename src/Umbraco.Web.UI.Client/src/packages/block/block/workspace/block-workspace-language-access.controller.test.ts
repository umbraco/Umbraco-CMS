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

const LANGUAGE_PERMISSION_PREFIX = 'UMB_LANGUAGE_PERMISSION_';
const BLOCK_MANAGER_RULE_UNIQUE = 'UMB_BLOCK_MANAGER_CONTEXT';

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

function findRule(manager: UmbReadOnlyVariantGuardManager, unique: string) {
	return manager.getRules().find((r) => r.unique === unique);
}

function expectRuleOnAllGuards(host: UmbTestBlockLanguageAccessHostElement, unique: string) {
	expect(findRule(host.workspaceContext.readOnlyGuard, unique), `workspace.readOnlyGuard rule "${unique}"`).to.exist;
	expect(findRule(host.workspaceContext.content.readOnlyGuard, unique), `content.readOnlyGuard rule "${unique}"`).to.exist;
	expect(findRule(host.workspaceContext.settings.readOnlyGuard, unique), `settings.readOnlyGuard rule "${unique}"`).to.exist;
}

function expectRuleAbsentFromAllGuards(host: UmbTestBlockLanguageAccessHostElement, unique: string) {
	expect(findRule(host.workspaceContext.readOnlyGuard, unique), `workspace.readOnlyGuard rule "${unique}"`).to.be.undefined;
	expect(findRule(host.workspaceContext.content.readOnlyGuard, unique), `content.readOnlyGuard rule "${unique}"`).to.be.undefined;
	expect(findRule(host.workspaceContext.settings.readOnlyGuard, unique), `settings.readOnlyGuard rule "${unique}"`).to.be.undefined;
}

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

	describe('Initial state', () => {
		it('sets fallbackToPermitted on all three guards before any rules are added', async () => {
			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			// With no rules and fallbackToPermitted, getIsPermittedForVariant returns true for any variant.
			expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.true;
			expect(host.workspaceContext.content.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.true;
			expect(host.workspaceContext.settings.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.true;
		});
	});

	describe('Invariant variantId — block manager state', () => {
		it('does not add the block-manager rule while manager.permitted is true (block is read-only)', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectRuleAbsentFromAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);
		});

		it('adds a permitted:false rule on all three guards when manager.permitted is false (block is editable)', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectRuleOnAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);
			const rule = findRule(host.workspaceContext.readOnlyGuard, BLOCK_MANAGER_RULE_UNIQUE)!;
			expect(rule.permitted).to.equal(false);
			expect(rule.variantId?.compare(UmbVariantId.INVARIANT)).to.be.true;
		});

		it('removes the block-manager rule when manager flips back to permitted:true', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectRuleOnAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);

			host.blockManagerContext.setPermitted(true);
			await flushMicrotasks();
			expectRuleAbsentFromAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);
		});

		it('re-adds the rule when manager flips permitted:true → false', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(true);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectRuleAbsentFromAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);

			host.blockManagerContext.setPermitted(false);
			await flushMicrotasks();
			expectRuleOnAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);
		});
	});

	describe('Variant block — language access', () => {
		it('adds permitted:false rule when hasAccessToAllLanguages is true', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			const unique = LANGUAGE_PERMISSION_PREFIX + 'en-US';
			expectRuleOnAllGuards(host, unique);
			const rule = findRule(host.workspaceContext.readOnlyGuard, unique)!;
			expect(rule.permitted).to.equal(false);
			expect(rule.variantId?.compare(enUS)).to.be.true;
		});

		it('adds permitted:false rule when culture is in user allowedLanguages', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages(['en-US']);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectRuleOnAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');
		});

		it('does not add a rule when culture is not in user allowedLanguages', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages(['da-DK']);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectRuleAbsentFromAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');
			expectRuleAbsentFromAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'da-DK');
		});

		it('does not add a rule when user has neither global access nor a matching language', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(false);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			expectRuleAbsentFromAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');
		});
	});

	describe('Cleanup transitions', () => {
		it('removes the prior culture rule when the variantId switches culture (en-US → da-DK)', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectRuleOnAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');

			host.workspaceContext.setVariantId(daDK);
			await flushMicrotasks();

			// The stale en-US rule must be cleaned up; only the da-DK rule should remain.
			expectRuleAbsentFromAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');
			expectRuleOnAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'da-DK');
		});

		it('clears the block-manager rule and observer when variantId switches from invariant → variant', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectRuleOnAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);

			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			expectRuleAbsentFromAllGuards(host, BLOCK_MANAGER_RULE_UNIQUE);
		});

		it('does not duplicate the block-manager rule when an invariant variantId re-emits (Phase 2 leak fix)', async () => {
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			host.blockManagerContext.setPermitted(false);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();

			// Force a re-emit by going variant → invariant. Each invariant emission would
			// previously leak a context consumer because the prior #consumeBlockManager
			// reference was overwritten without being destroyed.
			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			await flushMicrotasks();
			host.workspaceContext.setVariantId(enUS);
			await flushMicrotasks();
			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			await flushMicrotasks();

			// Exactly one block-manager rule should be present (deduped by unique key).
			const rules = host.workspaceContext.readOnlyGuard
				.getRules()
				.filter((r) => r.unique === BLOCK_MANAGER_RULE_UNIQUE);
			expect(rules.length).to.equal(1);
		});

		it('removes the language rule when culture becomes invariant', async () => {
			host.workspaceContext.setVariantId(enUS);
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.currentUserContext.setLanguages([]);

			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
			expectRuleOnAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');

			host.workspaceContext.setVariantId(UmbVariantId.CreateInvariant());
			await flushMicrotasks();
			expectRuleAbsentFromAllGuards(host, LANGUAGE_PERMISSION_PREFIX + 'en-US');
		});
	});
});
