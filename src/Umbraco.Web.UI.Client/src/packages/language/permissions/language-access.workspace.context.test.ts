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
		this.setVariants(cultures.map((culture) => new UmbVariantId(culture)));
	}

	setVariants(variantIds: Array<UmbVariantId>) {
		this.#variantOptions.setValue(
			variantIds.map(
				(variantId) =>
					({
						culture: variantId.culture,
						segment: variantId.segment,
						unique: variantId.toString(),
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

	describe('with segment variants of a culture the user has no access to', () => {
		const enUSSegment = new UmbVariantId('en-US', 'segment');

		beforeEach(() => {
			host.workspaceContext.setVariants([enUS, enUSSegment, daDK]);
			host.workspaceContext.setContentTypeProperties([
				{ unique: 'variant-property', variesByCulture: true, variesBySegment: false },
				{ unique: 'invariant-property', variesByCulture: false, variesBySegment: false },
			]);
		});

		it('denies writing variant properties in every segment of the culture with invariant-for-variant access', async () => {
			host.currentUserContext.setHasAccessToInvariantForVariant(true);
			await createContext();

			const propertyWriteGuard = host.workspaceContext.propertyWriteGuard;
			expect(propertyWriteGuard.getIsPermittedForVariantAndProperty(enUS, { unique: 'variant-property' }, enUS)).to.be
				.false;
			expect(
				propertyWriteGuard.getIsPermittedForVariantAndProperty(enUS, { unique: 'variant-property' }, enUSSegment),
			).to.be.false;
		});

		it('makes every segment of the culture read-only without invariant-for-variant access', async () => {
			host.currentUserContext.setHasAccessToInvariantForVariant(false);
			await createContext();

			expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(enUS)).to.be.true;
			expect(host.workspaceContext.readOnlyGuard.getIsPermittedForVariant(enUSSegment)).to.be.true;
		});
	});

	describe('with invariant-for-variant access and no language access', () => {
		const enUSSegment = new UmbVariantId('en-US', 'segment');
		const daDKSegment = new UmbVariantId('da-DK', 'segment');

		beforeEach(() => {
			host.workspaceContext.setVariants([enUS, enUSSegment, daDK, daDKSegment]);
			host.workspaceContext.setContentTypeProperties([
				{ unique: 'culture-property', variesByCulture: true, variesBySegment: false },
				{ unique: 'culture-and-segment-property', variesByCulture: true, variesBySegment: true },
				{ unique: 'segment-property', variesByCulture: false, variesBySegment: true },
				{ unique: 'invariant-property', variesByCulture: false, variesBySegment: false },
			]);
			host.currentUserContext.setLanguages([]);
			host.currentUserContext.setHasAccessToInvariantForVariant(true);
		});

		it('permits writing properties that do not vary by culture in every culture and segment', async () => {
			await createContext();

			const propertyWriteGuard = host.workspaceContext.propertyWriteGuard;
			for (const datasetVariantId of [enUS, enUSSegment, daDK, daDKSegment]) {
				expect(
					propertyWriteGuard.getIsPermittedForVariantAndProperty(
						new UmbVariantId(null, datasetVariantId.segment),
						{ unique: 'segment-property' },
						datasetVariantId,
					),
					`segment-property in ${datasetVariantId.toString()}`,
				).to.be.true;
				expect(
					propertyWriteGuard.getIsPermittedForVariantAndProperty(
						UmbVariantId.CreateInvariant(),
						{ unique: 'invariant-property' },
						datasetVariantId,
					),
					`invariant-property in ${datasetVariantId.toString()}`,
				).to.be.true;
			}
		});

		it('denies writing properties that vary by culture in every culture and segment', async () => {
			await createContext();

			const propertyWriteGuard = host.workspaceContext.propertyWriteGuard;
			for (const datasetVariantId of [enUS, enUSSegment, daDK, daDKSegment]) {
				expect(
					propertyWriteGuard.getIsPermittedForVariantAndProperty(
						new UmbVariantId(datasetVariantId.culture),
						{ unique: 'culture-property' },
						datasetVariantId,
					),
					`culture-property in ${datasetVariantId.toString()}`,
				).to.be.false;
				expect(
					propertyWriteGuard.getIsPermittedForVariantAndProperty(
						datasetVariantId,
						{ unique: 'culture-and-segment-property' },
						datasetVariantId,
					),
					`culture-and-segment-property in ${datasetVariantId.toString()}`,
				).to.be.false;
			}
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
