import { UmbLanguageAccessWorkspaceContext } from './language-access.workspace.context.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '@umbraco-cms/backoffice/content';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UmbReadOnlyVariantGuardManager, UmbVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

class UmbContentWorkspaceContextStub extends UmbContextBase {
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true;
	readonly #variantOptions = new UmbArrayState<UmbEntityVariantOptionModel>([], (x) => x.unique);
	readonly variantOptions = this.#variantOptions.asObservable();
	readonly #contentTypeProperties = new UmbArrayState<UmbPropertyTypeModel>([], (x) => x.unique);
	public readonly structure = { contentTypeProperties: this.#contentTypeProperties.asObservable() };
	public readonly readOnlyGuard = new UmbReadOnlyVariantGuardManager(this);
	public readonly propertyWriteGuard = new UmbVariantPropertyGuardManager(this);
	public readonly variantWriteGuard = new UmbVariantGuardManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CONTENT_WORKSPACE_CONTEXT.toString());
		this.propertyWriteGuard.fallbackToPermitted();
		this.variantWriteGuard.fallbackToPermitted();
	}

	setCultures(cultures: Array<string>) {
		this.#variantOptions.setValue(
			cultures.map(
				(culture) =>
					({
						culture,
						segment: null,
						unique: new UmbVariantId(culture).toString(),
					}) as UmbEntityVariantOptionModel,
			),
		);
	}

	setContentTypeProperties(properties: Array<Partial<UmbPropertyTypeModel>>) {
		this.#contentTypeProperties.setValue(properties as Array<UmbPropertyTypeModel>);
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

	setLanguages(languages: Array<string>) {
		this.#languages.setValue(languages);
	}

	setHasAccessToAllLanguages(value: boolean) {
		this.#hasAccessToAllLanguages.setValue(value);
	}

	setHasAccessToInvariantForVariant(value: boolean) {
		this.#hasAccessToInvariantForVariant.setValue(value);
	}
}

@customElement('umb-test-language-access-workspace-host')
class UmbTestLanguageAccessWorkspaceHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbContentWorkspaceContextStub;
	currentUserContext!: UmbCurrentUserContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbContentWorkspaceContextStub(this);
		this.currentUserContext = new UmbCurrentUserContextStub(this);
	}
}

const enUS = new UmbVariantId('en-US');
const daDK = new UmbVariantId('da-DK');

async function flushMicrotasks() {
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((r) => setTimeout(r, 0));
}

describe('UmbLanguageAccessWorkspaceContext', () => {
	let host: UmbTestLanguageAccessWorkspaceHostElement;

	beforeEach(async () => {
		host = await fixture(html`<umb-test-language-access-workspace-host></umb-test-language-access-workspace-host>`);
		host.workspaceContext.setCultures(['en-US', 'da-DK']);
		host.workspaceContext.setContentTypeProperties([
			{ unique: 'variant-property', variesByCulture: true, variesBySegment: false },
			{ unique: 'invariant-property', variesByCulture: false, variesBySegment: false },
		]);
		host.currentUserContext.setLanguages(['da-DK']);
		host.currentUserContext.setHasAccessToAllLanguages(false);
	});

	afterEach(() => {
		host.remove();
	});

	async function createContext() {
		new UmbLanguageAccessWorkspaceContext(host as unknown as UmbControllerHost);
		await flushMicrotasks();
	}

	describe('with invariant-for-variant access', () => {
		beforeEach(() => host.currentUserContext.setHasAccessToInvariantForVariant(true));

		it('denies writing a culture the user has no access to', async () => {
			await createContext();

			expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(enUS)).to.be.false;
		});

		it('keeps a culture the user has no access to editable, so invariant properties can be edited', async () => {
			await createContext();

			expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.false;
		});

		it('permits writing a culture the user has access to', async () => {
			await createContext();

			expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(daDK)).to.be.true;
		});
	});

	describe('without invariant-for-variant access', () => {
		beforeEach(() => host.currentUserContext.setHasAccessToInvariantForVariant(false));

		it('denies writing a culture the user has no access to', async () => {
			await createContext();

			expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(enUS)).to.be.false;
		});

		it('makes a culture the user has no access to read-only', async () => {
			await createContext();

			expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.true;
		});

		it('permits writing a culture the user has access to', async () => {
			await createContext();

			expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(daDK)).to.be.true;
		});
	});

	it('removes the write restriction when the user gains access to all languages', async () => {
		host.currentUserContext.setHasAccessToInvariantForVariant(true);
		await createContext();

		host.currentUserContext.setHasAccessToAllLanguages(true);
		await flushMicrotasks();

		expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(enUS)).to.be.true;
	});
});
