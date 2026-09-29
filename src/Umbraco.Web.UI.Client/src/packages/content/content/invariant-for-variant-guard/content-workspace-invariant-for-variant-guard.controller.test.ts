import { UmbContentWorkspaceInvariantForVariantGuardController } from './content-workspace-invariant-for-variant-guard.controller.js';
import { UMB_CONTENT_WORKSPACE_CONTEXT } from '../workspace/content-workspace.context-token.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UMB_CURRENT_USER_CONTEXT } from '@umbraco-cms/backoffice/current-user';
import { UmbVariantPropertyGuardManager } from '@umbraco-cms/backoffice/property';
import { UmbVariantGuardManager } from '@umbraco-cms/backoffice/utils';
import { UmbVariantId, type UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { UmbArrayState, UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

class UmbContentWorkspaceContextStub extends UmbContextBase {
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true;
	readonly #variantOptions = new UmbArrayState<UmbEntityVariantOptionModel>([], (x) => x.unique);
	readonly variantOptions = this.#variantOptions.asObservable();
	readonly #contentTypeProperties = new UmbArrayState<UmbPropertyTypeModel>([], (x) => x.unique);
	readonly #variesByCulture = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly structure = {
		contentTypeProperties: this.#contentTypeProperties.asObservable(),
		variesByCulture: this.#variesByCulture.asObservable(),
	};
	public readonly propertyWriteGuard = new UmbVariantPropertyGuardManager(this);
	public readonly variantWriteGuard = new UmbVariantGuardManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_CONTENT_WORKSPACE_CONTEXT.toString());
		this.propertyWriteGuard.fallbackToPermitted();
		this.variantWriteGuard.fallbackToPermitted();
		this.#variantOptions.setValue([
			{ culture: 'en-US', segment: null, unique: new UmbVariantId('en-US').toString() } as UmbEntityVariantOptionModel,
		]);
		this.#contentTypeProperties.setValue([
			{ unique: 'invariant-property', variesByCulture: false, variesBySegment: false } as UmbPropertyTypeModel,
		]);
	}

	setVariesByCulture(value: boolean) {
		this.#variesByCulture.setValue(value);
	}
}

class UmbCurrentUserContextStub extends UmbContextBase {
	readonly #hasAccessToInvariantForVariant = new UmbBooleanState<boolean | undefined>(undefined);
	public readonly hasAccessToInvariantForVariant = this.#hasAccessToInvariantForVariant.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_CURRENT_USER_CONTEXT.toString());
	}

	setHasAccessToInvariantForVariant(value: boolean) {
		this.#hasAccessToInvariantForVariant.setValue(value);
	}
}

@customElement('umb-test-invariant-for-variant-guard-host')
class UmbTestInvariantForVariantGuardHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	workspaceContext!: UmbContentWorkspaceContextStub;
	currentUserContext!: UmbCurrentUserContextStub;

	override connectedCallback() {
		super.connectedCallback();
		this.workspaceContext = new UmbContentWorkspaceContextStub(this);
		this.currentUserContext = new UmbCurrentUserContextStub(this);
	}
}

async function flushMicrotasks() {
	await Promise.resolve();
	await Promise.resolve();
	await new Promise((r) => setTimeout(r, 0));
}

describe('UmbContentWorkspaceInvariantForVariantGuardController', () => {
	let host: UmbTestInvariantForVariantGuardHostElement;
	const invariant = UmbVariantId.CreateInvariant();

	beforeEach(async () => {
		host = await fixture(
			html`<umb-test-invariant-for-variant-guard-host></umb-test-invariant-for-variant-guard-host>`,
		);
	});

	afterEach(() => {
		host.remove();
	});

	async function createController() {
		new UmbContentWorkspaceInvariantForVariantGuardController(host as unknown as UmbControllerHost);
		await flushMicrotasks();
	}

	it('denies writing the invariant variant of culture variant content without invariant-for-variant access', async () => {
		host.workspaceContext.setVariesByCulture(true);
		host.currentUserContext.setHasAccessToInvariantForVariant(false);
		await createController();

		expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(invariant)).to.be.false;
	});

	it('permits writing the invariant variant of culture variant content with invariant-for-variant access', async () => {
		host.workspaceContext.setVariesByCulture(true);
		host.currentUserContext.setHasAccessToInvariantForVariant(true);
		await createController();

		expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(invariant)).to.be.true;
	});

	it('permits writing the invariant variant of culture invariant content without invariant-for-variant access', async () => {
		host.workspaceContext.setVariesByCulture(false);
		host.currentUserContext.setHasAccessToInvariantForVariant(false);
		await createController();

		expect(host.workspaceContext.variantWriteGuard.getIsPermittedForVariant(invariant)).to.be.true;
	});
});
