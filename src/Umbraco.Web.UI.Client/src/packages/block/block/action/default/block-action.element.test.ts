import { UmbBlockActionDefaultElement } from './block-action.element.js';
import type { ManifestBlockAction } from '../block-action.extension.js';
import type { MetaBlockActionDefaultKind } from './types.js';
import type { UmbBlockAction } from '../block-action.interface.js';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';
import { UmbBasicState } from '@umbraco-cms/backoffice/observable-api';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';

const VALIDATION_PATH = '$.contentData[0]';

/** Api implementing the modern, reactive validation path variant. */
class UmbTestObservableBlockAction {
	#validationPath = new UmbBasicState<string | undefined>(undefined);

	async getHref() {
		return undefined;
	}

	async execute() {}

	async getValidationDataPath() {
		return undefined;
	}

	async getValidationDataPathObservable() {
		return this.#validationPath.asObservable();
	}

	setValidationPath(path: string | undefined) {
		this.#validationPath.setValue(path);
	}
}

/** Api implementing only the deprecated, one-shot validation path getter (no observable variant). */
class UmbTestLegacyBlockAction {
	#path: string | undefined;

	async getHref() {
		return undefined;
	}

	async execute() {}

	async getValidationDataPath() {
		return this.#path;
	}

	setValidationPath(path: string | undefined) {
		this.#path = path;
	}
}

const manifest = {
	type: 'blockAction',
	kind: 'default',
	alias: 'Test.BlockAction.Default',
	name: 'Test Block Action',
	weight: 0,
	meta: { icon: 'icon-test', label: 'Test Action' },
} as unknown as ManifestBlockAction<MetaBlockActionDefaultKind>;

function isInvalid(element: UmbBlockActionDefaultElement): boolean {
	return element.shadowRoot!.querySelector('uui-badge') !== null;
}

describe('UmbBlockActionDefaultElement', () => {
	let element: UmbBlockActionDefaultElement;
	let warnCalls: Array<unknown[]>;
	const originalWarn = console.warn;

	beforeEach(async () => {
		warnCalls = [];
		console.warn = (...args: unknown[]) => {
			warnCalls.push(args);
		};

		element = await fixture(html`<umb-block-action></umb-block-action>`);
		element.manifest = manifest;

		// The validation state controller consumes UMB_VALIDATION_CONTEXT; provide it on the element
		// itself and register a message at VALIDATION_PATH, so a subscription at that path resolves invalid.
		const validationContext = new UmbValidationContext(element);
		validationContext.messages.addMessage('server', VALIDATION_PATH, 'Invalid');
	});

	afterEach(() => {
		console.warn = originalWarn;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbBlockActionDefaultElement);
	});

	it('shows the invalid badge while the validation path observable resolves a path, and clears it once the path is removed', async () => {
		const api = new UmbTestObservableBlockAction();
		element.api = api as unknown as UmbBlockAction<MetaBlockActionDefaultKind>;
		await aTimeout(0);
		expect(isInvalid(element)).to.be.false;

		api.setValidationPath(VALIDATION_PATH);
		await aTimeout(0);
		expect(isInvalid(element)).to.be.true;

		api.setValidationPath(undefined);
		await aTimeout(0);
		expect(isInvalid(element)).to.be.false;

		expect(warnCalls).to.have.lengthOf(0);
	});

	// TODO: Remove this test once the legacy getValidationDataPath is removed. [NL]
	it('falls back to the legacy getValidationDataPath and warns once when no observable variant exists', async () => {
		const api = new UmbTestLegacyBlockAction();
		api.setValidationPath(VALIDATION_PATH);
		element.api = api as unknown as UmbBlockAction<MetaBlockActionDefaultKind>;
		await aTimeout(0);

		expect(isInvalid(element)).to.be.true;
		expect(warnCalls).to.have.lengthOf(1);
	});

	it('does not warn when the legacy getValidationDataPath resolves no path', async () => {
		const api = new UmbTestLegacyBlockAction();
		element.api = api as unknown as UmbBlockAction<MetaBlockActionDefaultKind>;
		await aTimeout(0);

		expect(isInvalid(element)).to.be.false;
		expect(warnCalls).to.have.lengthOf(0);
	});
});
