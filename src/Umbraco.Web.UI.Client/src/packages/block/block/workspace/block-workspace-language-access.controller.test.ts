import { UMB_BLOCK_MANAGER_CONTEXT } from '../context/block-manager.context-token.js';
import { UMB_BLOCK_WORKSPACE_CONTEXT } from './block-workspace.context-token.js';
import { UmbBlockLanguageAccessWorkspaceController } from './block-workspace-language-access.controller.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UMB_PROPERTY_CONTEXT, UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UmbReadOnlyVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, UmbBasicState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

class UmbBlockWorkspaceContextStub extends UmbContextBase {
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;
	readonly #variantId = new UmbBasicState<UmbVariantId | undefined>(undefined);
	readonly variantId = this.#variantId.asObservable();
	readonly #contentVariesByCulture = new UmbBooleanState<boolean | undefined>(undefined);
	readonly #settingsVariesByCulture = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly readOnlyGuard = new UmbReadOnlyVariantGuardManager(this);
	public readonly content = {
		readOnlyGuard: new UmbReadOnlyVariantGuardManager(this),
		propertyWriteGuard: new UmbVariantPropertyGuardManager(this),
		structure: { variesByCulture: this.#contentVariesByCulture.asObservable() },
	};
	public readonly settings = {
		readOnlyGuard: new UmbReadOnlyVariantGuardManager(this),
		propertyWriteGuard: new UmbVariantPropertyGuardManager(this),
		structure: { variesByCulture: this.#settingsVariesByCulture.asObservable() },
	};

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_WORKSPACE_CONTEXT.toString());
		this.content.propertyWriteGuard.fallbackToPermitted();
		this.settings.propertyWriteGuard.fallbackToPermitted();
	}

	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
	}

	setElementVariesByCulture(value: boolean) {
		this.#contentVariesByCulture.setValue(value);
		this.#settingsVariesByCulture.setValue(value);
	}
}

class UmbOwnerContentWorkspaceContextStub extends UmbContextBase {
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true;
	readonly #variantOptions = new UmbArrayState<UmbEntityVariantOptionModel>([], (x) => x.unique);
	readonly variantOptions = this.#variantOptions.asObservable();
	readonly #variesByCulture = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly structure = { variesByCulture: this.#variesByCulture.asObservable() };

	constructor(host: UmbControllerHost) {
		super(host, UMB_CONTENT_WORKSPACE_CONTEXT.toString());
	}

	setCultures(cultures: Array<string>) {
		this.#variantOptions.setValue(
			cultures.map(
				(culture) =>
					({ culture, segment: null, unique: new UmbVariantId(culture).toString() }) as UmbEntityVariantOptionModel,
			),
		);
	}

	setVariesByCulture(value: boolean) {
		this.#variesByCulture.setValue(value);
	}
}

class UmbVaryingPropertyContextStub extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_CONTEXT.toString());
	}

	getVariantId() {
		return new UmbVariantId('en-US');
	}
}

class UmbBlockManagerContextStub extends UmbContextBase {
	readonly #permitted = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly readOnlyState = { permitted: this.#permitted.asObservable() };
	readonly #variantId = new UmbBasicState<UmbVariantId | undefined>(undefined);
	public readonly variantId = this.#variantId.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_MANAGER_CONTEXT.toString());
	}

	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
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
	readonly #hasAccessToInvariantForVariant = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly hasAccessToInvariantForVariant = this.#hasAccessToInvariantForVariant.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT.toString());
	}

	setHasAccessToInvariantForVariant(value: boolean) {
		this.#hasAccessToInvariantForVariant.setValue(value);
	}

	setLanguages(languages: Array<string>) {
		this.#languages.setValue(languages);
	}

	setHasAccessToAllLanguages(value: boolean) {
		this.#hasAccessToAllLanguages.setValue(value);
	}
}

/**
 * The block workspace is nested inside its owner (a content workspace, and optionally a culture-varying property).
 * They share a context alias with the block workspace, so they must live on a parent element.
 */
@customElement('umb-test-block-owner-host')
class UmbTestBlockOwnerHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	provideOwnerWorkspaceContext() {
		return new UmbOwnerContentWorkspaceContextStub(this);
	}

	provideVaryingPropertyContext() {
		return new UmbVaryingPropertyContextStub(this);
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
	let ownerHost: UmbTestBlockOwnerHostElement;
	let host: UmbTestBlockLanguageAccessHostElement;

	beforeEach(async () => {
		ownerHost = await fixture(html`
			<umb-test-block-owner-host>
				<umb-test-block-language-access-host></umb-test-block-language-access-host>
			</umb-test-block-owner-host>
		`);
		host = ownerHost.querySelector('umb-test-block-language-access-host') as UmbTestBlockLanguageAccessHostElement;
	});

	afterEach(() => {
		ownerHost.remove();
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

	describe('Shared (invariant) properties', () => {
		const invariant = UmbVariantId.CreateInvariant();

		function isSharedPropertyWritable(datasetVariantId: UmbVariantId, part: 'content' | 'settings' = 'content') {
			return host.workspaceContext[part].propertyWriteGuard.getIsPermittedForVariantAndProperty(
				invariant,
				{ unique: 'shared-property' },
				datasetVariantId,
			);
		}

		function setUpOwner() {
			const owner = ownerHost.provideOwnerWorkspaceContext();
			owner.setCultures(['en-US', 'da-DK']);
			owner.setVariesByCulture(true);
			return owner;
		}

		async function createController() {
			new UmbBlockLanguageAccessWorkspaceController(host as unknown as UmbControllerHost);
			await flushMicrotasks();
		}

		beforeEach(() => {
			host.currentUserContext.setHasAccessToAllLanguages(true);
			host.workspaceContext.setVariantId(daDK);
			host.workspaceContext.setElementVariesByCulture(true);
		});

		it('denies writing shared properties in every variant of the owner without invariant-for-variant access', async () => {
			setUpOwner();
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(enUS), 'content en-US').to.be.false;
			expect(isSharedPropertyWritable(daDK), 'content da-DK').to.be.false;
			expect(isSharedPropertyWritable(enUS, 'settings'), 'settings en-US').to.be.false;
			expect(isSharedPropertyWritable(daDK, 'settings'), 'settings da-DK').to.be.false;
		});

		it('permits writing shared properties with invariant-for-variant access', async () => {
			setUpOwner();
			host.currentUserContext.setHasAccessToInvariantForVariant(true);
			await createController();

			expect(isSharedPropertyWritable(enUS)).to.be.true;
			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});

		it('does not restrict a block hosted by a property that varies by culture', async () => {
			setUpOwner();
			ownerHost.provideVaryingPropertyContext();
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(enUS)).to.be.true;
			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});

		it('does not restrict blocks of content that does not vary by culture', async () => {
			const owner = setUpOwner();
			owner.setVariesByCulture(false);
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});

		it('does not restrict blocks that are not hosted by a content workspace', async () => {
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});

		it('denies the invariant dataset of an element type that does not vary by culture', async () => {
			setUpOwner();
			host.workspaceContext.setElementVariesByCulture(false);
			host.blockManagerContext.setVariantId(daDK);
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(invariant), 'content invariant dataset').to.be.false;
			expect(isSharedPropertyWritable(invariant, 'settings'), 'settings invariant dataset').to.be.false;
		});

		it('leaves the invariant dataset of an element type that varies by culture alone', async () => {
			setUpOwner();
			host.blockManagerContext.setVariantId(daDK);
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			expect(isSharedPropertyWritable(invariant)).to.be.true;
		});

		it('removes the restriction when the user gains invariant-for-variant access', async () => {
			setUpOwner();
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();
			expect(isSharedPropertyWritable(daDK)).to.be.false;

			host.currentUserContext.setHasAccessToInvariantForVariant(true);
			await flushMicrotasks();

			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});

		it('restricts again when the user loses invariant-for-variant access', async () => {
			setUpOwner();
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();
			host.currentUserContext.setHasAccessToInvariantForVariant(true);
			await flushMicrotasks();

			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await flushMicrotasks();

			expect(isSharedPropertyWritable(daDK)).to.be.false;
		});

		it('does not restrict variants of the owner added after the user gains invariant-for-variant access', async () => {
			const owner = setUpOwner();
			owner.setCultures(['en-US']);
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createController();

			host.currentUserContext.setHasAccessToInvariantForVariant(true);
			await flushMicrotasks();
			owner.setCultures(['en-US', 'da-DK']);
			await flushMicrotasks();

			expect(isSharedPropertyWritable(daDK)).to.be.true;
		});
	});
});
