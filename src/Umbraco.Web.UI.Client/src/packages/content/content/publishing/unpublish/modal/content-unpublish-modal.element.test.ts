import type { UmbContentUnpublishModalElement } from './content-unpublish-modal.element.js';
import './content-unpublish-modal.element.js';
import type { UmbContentUnpublishModalData } from './types.js';
import type { UmbContentConfigurationModel, UmbContentConfigurationRepository } from '../../../configuration/types.js';
import type { UmbEntityReferenceRepository } from '@umbraco-cms/backoffice/relations';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

import '@umbraco-cms/backoffice/external/uui';
import '@umbraco-cms/backoffice/localization';

const REFERENCE_REPOSITORY_ALIAS = 'Umb.Test.ContentUnpublishModal.ReferenceRepository';
const CONFIGURATION_REPOSITORY_ALIAS = 'Umb.Test.ContentUnpublishModal.ConfigurationRepository';

const REFERENCES_DATA = {
	unique: 'elm-1',
	itemRepositoryAlias: 'n/a',
	referenceRepositoryAlias: REFERENCE_REPOSITORY_ALIAS,
};

const INVARIANT_OPTION: UmbEntityVariantOptionModel = {
	unique: 'invariant',
	culture: null,
	segment: null,
	language: {
		entityType: 'language',
		unique: 'en-us',
		name: 'English',
		isDefault: true,
		isMandatory: true,
		fallbackIsoCode: null,
	},
};

class UmbTestReferenceRepository implements UmbEntityReferenceRepository {
	static total = 0;
	// Resolved by default so the reference check completes immediately; a test can replace it with a pending
	// promise to observe the state while the check is still in flight.
	static gate: Promise<void> = Promise.resolve();

	async requestReferencedBy() {
		await UmbTestReferenceRepository.gate;
		return { data: { items: [], total: UmbTestReferenceRepository.total } };
	}

	async requestAreReferenced() {
		return { data: { items: [], total: 0 } };
	}

	async requestDescendantsWithReferences() {
		return { data: { items: [], total: 0 } };
	}

	destroy() {}
}

class UmbTestConfigurationRepository implements UmbContentConfigurationRepository {
	static configuration: UmbContentConfigurationModel = { disableUnpublishWhenReferenced: false };

	async requestConfiguration() {
		return { data: UmbTestConfigurationRepository.configuration };
	}

	destroy() {}
}

describe('UmbContentUnpublishModalElement', () => {
	let element: UmbContentUnpublishModalElement;

	before(() => {
		const referenceManifest: ManifestApi<UmbTestReferenceRepository> = {
			type: 'my-test-type',
			alias: REFERENCE_REPOSITORY_ALIAS,
			name: 'Test Entity Reference Repository',
			api: UmbTestReferenceRepository,
		};
		umbExtensionsRegistry.register(referenceManifest);

		const configurationManifest: ManifestApi<UmbTestConfigurationRepository> = {
			type: 'my-test-type',
			alias: CONFIGURATION_REPOSITORY_ALIAS,
			name: 'Test Content Configuration Repository',
			api: UmbTestConfigurationRepository,
		};
		umbExtensionsRegistry.register(configurationManifest);
	});

	after(() => {
		umbExtensionsRegistry.unregister(REFERENCE_REPOSITORY_ALIAS);
		umbExtensionsRegistry.unregister(CONFIGURATION_REPOSITORY_ALIAS);
	});

	beforeEach(() => {
		UmbTestReferenceRepository.total = 0;
		UmbTestReferenceRepository.gate = Promise.resolve();
		UmbTestConfigurationRepository.configuration = { disableUnpublishWhenReferenced: false };
		element = document.createElement('umb-content-unpublish-modal') as UmbContentUnpublishModalElement;
	});

	afterEach(() => {
		element.remove();
	});

	function getPrompt() {
		return element.shadowRoot?.querySelector('umb-localize') ?? null;
	}

	function getUnpublishButton() {
		return element.shadowRoot?.querySelectorAll('uui-button')[1];
	}

	async function open(data: Partial<UmbContentUnpublishModalData> = {}) {
		element.data = { options: [INVARIANT_OPTION], ...data };
		document.body.appendChild(element);
		await aTimeout(0);
	}

	async function openReferenced(disableUnpublishWhenReferenced: boolean) {
		UmbTestReferenceRepository.total = 2;
		UmbTestConfigurationRepository.configuration = { disableUnpublishWhenReferenced };
		await open({ ...REFERENCES_DATA, configurationRepositoryAlias: CONFIGURATION_REPOSITORY_ALIAS });
	}

	function expectPrompt(key: string, disabled: boolean) {
		expect(getPrompt()?.getAttribute('key')).to.equal(key);
		expect(getUnpublishButton()?.hasAttribute('disabled')).to.equal(disabled);
	}

	it('shows the confirm prompt and an enabled button when there is no references config', async () => {
		await open();

		expectPrompt('prompt_confirmUnpublish', false);
	});

	it('hides the prompt and disables the button while the reference check is pending', async () => {
		let resolveGate!: () => void;
		UmbTestReferenceRepository.gate = new Promise((resolve) => {
			resolveGate = resolve;
		});

		await open(REFERENCES_DATA);

		expect(getPrompt(), 'prompt should not be shown yet').to.be.null;
		expect(getUnpublishButton()?.hasAttribute('disabled'), 'button should be disabled while loading').to.be.true;

		resolveGate();
		await aTimeout(0);

		expectPrompt('prompt_confirmUnpublish', false);
	});

	it('blocks unpublishing when referenced and disableUnpublishWhenReferenced is set', async () => {
		await openReferenced(true);

		expectPrompt('prompt_cannotUnpublishWhenReferenced', true);
	});

	it('allows unpublishing when referenced but disableUnpublishWhenReferenced is not set', async () => {
		await openReferenced(false);

		expectPrompt('prompt_confirmUnpublish', false);
	});
});
