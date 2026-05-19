import { umbExtensionsRegistry } from './registry.js';
import { expect } from '@open-wc/testing';
import { UmbExtensionRegistry } from '@umbraco-cms/backoffice/extension-api';
import type { ManifestKind } from '@umbraco-cms/backoffice/extension-api';

// Aliases scoped to this test file so we don't collide with anything else
// that may already be registered in the shared singleton.
const SECTION_ALIAS_A = 'Umb.Test.Registry.Section.A';
const SECTION_ALIAS_B = 'Umb.Test.Registry.Section.B';
const KIND_ALIAS = 'Umb.Test.Registry.Kind';

describe('umbExtensionsRegistry (singleton)', () => {
	afterEach(() => {
		// Conservative cleanup: remove only the aliases we added, so the singleton
		// is left in the same state the suite found it in.
		[SECTION_ALIAS_A, SECTION_ALIAS_B, KIND_ALIAS].forEach((alias) => {
			if (umbExtensionsRegistry.isRegistered(alias)) {
				umbExtensionsRegistry.unregister(alias);
			}
		});
	});

	it('is an instance of UmbExtensionRegistry', () => {
		expect(umbExtensionsRegistry).to.be.instanceOf(UmbExtensionRegistry);
	});

	it('registers and reports an extension as registered, then unregisters it', () => {
		expect(umbExtensionsRegistry.isRegistered(SECTION_ALIAS_A)).to.be.false;

		umbExtensionsRegistry.register({
			type: 'section',
			name: 'Test Section A',
			alias: SECTION_ALIAS_A,
			meta: { label: 'Test Section A', pathname: 'test-a' },
		} as UmbExtensionManifest);

		expect(umbExtensionsRegistry.isRegistered(SECTION_ALIAS_A)).to.be.true;

		const ext = umbExtensionsRegistry.getByAlias(SECTION_ALIAS_A);
		expect(ext?.alias).to.equal(SECTION_ALIAS_A);

		umbExtensionsRegistry.unregister(SECTION_ALIAS_A);
		expect(umbExtensionsRegistry.isRegistered(SECTION_ALIAS_A)).to.be.false;
	});

	it('ignores duplicate registration of the same alias', () => {
		const manifest = {
			type: 'section',
			name: 'Test Section B (first)',
			alias: SECTION_ALIAS_B,
			meta: { label: 'first', pathname: 'b' },
		} as UmbExtensionManifest;

		umbExtensionsRegistry.register(manifest);

		// Suppress the expected console.error from the duplicate-registration check.
		const originalError = console.error;
		const errors: Array<unknown[]> = [];
		console.error = (...args) => errors.push(args);
		try {
			umbExtensionsRegistry.register({
				...manifest,
				name: 'Test Section B (second)',
			});
		} finally {
			console.error = originalError;
		}

		// The first registration is preserved, the second is rejected.
		const ext = umbExtensionsRegistry.getByAlias(SECTION_ALIAS_B) as { name: string };
		expect(ext?.name).to.equal('Test Section B (first)');
		expect(errors.length).to.equal(1);
	});

	it('merges a kind manifest into a registered extension that references it', () => {
		const kind: ManifestKind<UmbExtensionManifest> = {
			type: 'kind',
			alias: KIND_ALIAS,
			matchType: 'section',
			matchKind: 'test-kind',
			manifest: {
				type: 'section',
				name: 'kind-default-name',
				alias: 'kind-default-alias',
				meta: {
					label: 'label-from-kind',
					pathname: 'pathname-from-kind',
				},
			} as UmbExtensionManifest,
		};

		umbExtensionsRegistry.register(kind);
		umbExtensionsRegistry.register({
			type: 'section',
			kind: 'test-kind',
			name: 'consumer',
			alias: SECTION_ALIAS_A,
			weight: 10,
			meta: {
				// `label` deliberately omitted — should fall back to the kind's value.
				pathname: 'pathname-from-extension',
			},
		} as unknown as UmbExtensionManifest);

		const merged = umbExtensionsRegistry.getByAlias(SECTION_ALIAS_A) as {
			meta: { label: string; pathname: string };
		};

		// Kind contributes the missing field; consumer wins where it sets a value.
		expect(merged.meta.label).to.equal('label-from-kind');
		expect(merged.meta.pathname).to.equal('pathname-from-extension');
	});
});
