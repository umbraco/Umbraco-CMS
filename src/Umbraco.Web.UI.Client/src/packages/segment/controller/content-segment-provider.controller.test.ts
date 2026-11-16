import { UmbDocumentSegmentProvider } from './content-segment-provider.controller.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin, type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_CONTENT_WORKSPACE_CONTEXT, type UmbContentDetailIncomingDataHookMeta } from '@umbraco-cms/backoffice/content';
import { UmbParallelHookController } from '@umbraco-cms/backoffice/hook-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbArrayState, UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel, UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { UmbWorkspaceVariantOptionsController } from '@umbraco-cms/backoffice/workspace';
import { useMockSet } from '@umbraco-cms/internal/mock-manager';

const createLanguage = (unique: string, isDefault = false): UmbLanguageDetailModel => ({
	entityType: 'language',
	unique,
	name: unique,
	isDefault,
	isMandatory: false,
	fallbackIsoCode: null,
});

@customElement('umb-test-content-segment-provider-host')
class UmbTestContentSegmentProviderHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestContentWorkspaceContext {
	readonly IS_CONTENT_WORKSPACE_CONTEXT = true;
	readonly #variesByCulture = new UmbBasicState<boolean | undefined>(undefined);
	readonly variesByCulture = this.#variesByCulture.asObservable();
	readonly #variesBySegment = new UmbBasicState<boolean | undefined>(undefined);
	readonly variesBySegment = this.#variesBySegment.asObservable();
	readonly incomingDataHook: UmbParallelHookController<UmbContentDetailIncomingDataHookMeta>;
	readonly variantOptionsManager: UmbWorkspaceVariantOptionsController<UmbEntityVariantModel>;
	readonly #host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
		this.incomingDataHook = new UmbParallelHookController(host);
		this.variantOptionsManager = new UmbWorkspaceVariantOptionsController(
			host,
			this.variesByCulture,
			this.variesBySegment,
			new UmbBasicState<Array<UmbEntityVariantModel>>([]).asObservable(),
			new UmbArrayState<UmbLanguageDetailModel>([createLanguage('en-US', true), createLanguage('da')], (x) => x.unique).asObservable(),
		);
		new UmbContextProviderController(host, UMB_CONTENT_WORKSPACE_CONTEXT, this as any);
	}

	getHostElement() {
		return this.#host.getHostElement();
	}

	setVariation(byCulture: boolean, bySegment: boolean) {
		this.#variesByCulture.setValue(byCulture);
		this.#variesBySegment.setValue(bySegment);
	}
}

const settle = () => new Promise((resolve) => setTimeout(resolve, 0));

describe('UmbDocumentSegmentProvider', () => {
	let host: UmbTestContentSegmentProviderHostElement;
	let workspace: UmbTestContentWorkspaceContext;
	let provider: UmbDocumentSegmentProvider;
	let latest: Array<UmbEntityVariantOptionModel> | undefined;

	const segmentOptionUniques = () =>
		latest?.filter((option) => option.segment !== null).map((option) => option.unique) ?? [];

	const loadWith = async (byCulture: boolean, bySegment: boolean) => {
		workspace.setVariation(byCulture, bySegment);
		await workspace.incomingDataHook.execute({ entityType: 'document', unique: 'test', contentType: undefined });
		await settle();
	};

	beforeEach(async () => {
		await useMockSet('documents');
		host = await fixture(html`<umb-test-content-segment-provider-host></umb-test-content-segment-provider-host>`);
		workspace = new UmbTestContentWorkspaceContext(host);
		provider = new UmbDocumentSegmentProvider(host);
		latest = undefined;
		workspace.variantOptionsManager.observe(
			workspace.variantOptionsManager.variantOptions,
			(options) => {
				latest = options;
			},
			null,
		);
		await settle();
	});

	describe('when the content varies by culture and segment', () => {
		beforeEach(async () => {
			await loadWith(true, true);
		});

		it('adds every segment to every culture', () => {
			expect(segmentOptionUniques()).to.have.members(['en-US_s1', 'da_s1', 'en-US_s2', 'da_s2']);
		});
	});

	describe('when the content varies by segment only', () => {
		it('adds every segment without a culture', async () => {
			await loadWith(false, true);
			expect(segmentOptionUniques()).to.have.members(['invariant_s1', 'invariant_s2']);
		});
	});

	describe('when the content does not vary by segment', () => {
		it('adds no segment options', async () => {
			await loadWith(true, false);
			expect(segmentOptionUniques()).to.be.empty;
		});
	});

	it('replaces its segment options when the variation changes', async () => {
		await loadWith(true, true);

		workspace.setVariation(false, true);
		await settle();

		expect(segmentOptionUniques()).to.have.members(['invariant_s1', 'invariant_s2']);
	});

	it('removes its segment options when destroyed', async () => {
		await loadWith(true, true);
		expect(segmentOptionUniques()).to.not.be.empty;

		provider.destroy();
		await settle();

		expect(segmentOptionUniques()).to.be.empty;
	});
});
