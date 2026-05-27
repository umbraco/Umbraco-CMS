import { UmbInputTiptapElement } from './input-tiptap.element.js';
import { manifests as tiptapManifests } from '../../manifests.js';
import { expect, fixture, html } from '@open-wc/testing';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

describe('UmbInputTiptapElement (standalone)', () => {
	// Proves that `<umb-input-tiptap>` is usable on its own — i.e. not gated on being
	// rendered inside `<umb-property-editor-ui-tiptap>`. Stories cover the full visual
	// load path; this just nails down that the custom element is importable and the
	// package's manifests can be registered alongside it without any other plumbing.

	before(() => {
		umbExtensionsRegistry.registerMany(tiptapManifests);
	});

	after(() => {
		for (const m of tiptapManifests) {
			umbExtensionsRegistry.unregister(m.alias);
		}
	});

	it('is defined and instantiable on its own', async () => {
		const element = await fixture<UmbInputTiptapElement>(html`<umb-input-tiptap></umb-input-tiptap>`);
		expect(element).to.be.instanceOf(UmbInputTiptapElement);
	});

	it('has every first-party Tiptap manifest available in the registry after registration', () => {
		// The data-type configuration UIs read these manifests via `byType(...)`; the
		// boundary split changes only the `api`/`element` shape, never the metadata
		// the configuration UIs depend on. Spot-check that the registry has them.
		const knownAliases = ['Umb.Tiptap.RichTextEssentials', 'Umb.Tiptap.Bold', 'Umb.Tiptap.Table'];
		for (const alias of knownAliases) {
			expect(umbExtensionsRegistry.getByAlias(alias), `manifest ${alias} should be registered`).to.exist;
		}
	});
});
