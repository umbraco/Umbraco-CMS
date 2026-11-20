import type { UmbContentUnpublishModalElement } from './content-unpublish-modal.element.js';
import './content-unpublish-modal.element.js';
import type { UmbContentUnpublishModalData, UmbContentUnpublishModalValue } from './types.js';
import type { UmbContentConfigurationModel, UmbContentConfigurationRepository } from '../../../configuration/types.js';
import type { UmbEntityReferenceRepository } from '@umbraco-cms/backoffice/relations';
import type { UmbEntityVariantOptionModel } from '@umbraco-cms/backoffice/variant';
import { aTimeout, expect } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbPublishableVariantState } from '@umbraco-cms/backoffice/variant';
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

const option = (culture: string, isMandatory = false): UmbEntityVariantOptionModel =>
	({
		unique: culture,
		culture,
		segment: null,
		language: {
			unique: culture,
			name: culture,
			entityType: 'language',
			isDefault: false,
			isMandatory,
			fallbackIsoCode: null,
		},
		variant: { culture, segment: null, name: culture, state: UmbPublishableVariantState.PUBLISHED },
	}) as unknown as UmbEntityVariantOptionModel;

describe('UmbContentUnpublishModalElement language selection', () => {
	let element: UmbContentUnpublishModalElement;
	let submitted: UmbContentUnpublishModalValue | undefined;

	const render = async (selection: Array<string>, data: Partial<UmbContentUnpublishModalData> = {}) => {
		submitted = undefined;
		element = document.createElement('umb-content-unpublish-modal');
		element.modalContext = {
			value: new UmbObjectState<UmbContentUnpublishModalValue>({ selection }).asObservable(),
			setValue: (value: UmbContentUnpublishModalValue) => (submitted = value),
			submit: () => {},
			reject: () => {},
		} as any;
		element.data = { options: [option('en-US', true), option('da-DK'), option('de-DE')], ...data };
		document.body.appendChild(element);
		await element.updateComplete;
		await aTimeout(0);
	};

	const item = (label: string) => {
		const picker = element.shadowRoot!.querySelector('umb-content-variant-language-picker')!;
		const menuItem = picker.shadowRoot!.querySelector<HTMLElement>(`uui-menu-item[label="${label}"]`);
		expect(menuItem, label).to.exist;
		return menuItem!;
	};

	const toggle = async (label: string, event: 'selected' | 'deselected') => {
		item(label).dispatchEvent(new Event(event));
		await aTimeout(0);
	};

	const submit = async () => {
		element.shadowRoot!.querySelector<HTMLElement>('uui-button[look="primary"]')!.click();
		await aTimeout(0);
		return [...(submitted?.selection ?? [])].sort();
	};

	afterEach(() => element.remove());

	it('selects every other language when the initial selection includes a mandatory language', async () => {
		await render(['en-US']);

		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('selects every other language when a mandatory language is selected', async () => {
		await render(['da-DK']);
		await toggle('en-US', 'selected');

		expect(item('de-DE').hasAttribute('selected'), 'shown as selected').to.be.true;
		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('keeps every other language selected while a mandatory language is selected', async () => {
		await render(['en-US']);
		item('da-DK').shadowRoot!.querySelector<HTMLElement>('#label-button')!.click();
		await aTimeout(0);

		expect(item('da-DK').hasAttribute('selected'), 'shown as selected').to.be.true;
		expect(await submit()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});

	it('deselects only the languages it selected when the mandatory language is deselected', async () => {
		await render(['da-DK']);
		await toggle('en-US', 'selected');
		await toggle('en-US', 'deselected');

		expect(await submit()).to.deep.equal(['da-DK']);
	});

	it('keeps every language selected when an automatically selected mandatory language is deselected', async () => {
		await render(['en-US'], { options: [option('en-US', true), option('it-IT', true), option('fr-FR')] });
		await toggle('it-IT', 'deselected');

		expect(await submit()).to.deep.equal(['en-US', 'fr-FR', 'it-IT']);
	});

	it('deselects every mandatory language when one the editor selected is deselected', async () => {
		await render(['en-US', 'it-IT', 'da-DK'], {
			options: [option('en-US', true), option('it-IT', true), option('da-DK'), option('de-DE')],
		});
		await toggle('it-IT', 'deselected');

		expect(await submit()).to.deep.equal(['da-DK']);
	});

	it('explains that unpublishing a mandatory language unpublishes every language', async () => {
		await render(['da-DK']);

		expect(element.shadowRoot!.querySelector('umb-localize[key="content_languagesToUnpublish"]')).to.exist;
	});

	it('leaves the selection alone when no mandatory language is selected', async () => {
		await render(['da-DK']);

		expect(await submit()).to.deep.equal(['da-DK']);
	});

	it('does not select languages the user cannot pick', async () => {
		await render(['en-US'], { pickableFilter: (o) => o.culture !== 'de-DE' });

		expect(await submit()).to.deep.equal(['da-DK', 'en-US']);
	});

	it('emits the selected languages consistently to every observer of the selection', async () => {
		await render(['da-DK']);
		let observed: Array<string> = [];
		const subscription = (element as any)._selectionManager.selection.subscribe(
			(selection: Array<string>) => (observed = selection),
		);
		await toggle('en-US', 'selected');
		subscription.unsubscribe();

		expect([...observed].sort()).to.deep.equal(['da-DK', 'de-DE', 'en-US']);
	});
});
