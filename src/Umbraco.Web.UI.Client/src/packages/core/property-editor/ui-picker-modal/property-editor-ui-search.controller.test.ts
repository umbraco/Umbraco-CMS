import { expect } from '@open-wc/testing';
import { UmbPropertyEditorUISearchController } from './property-editor-ui-search.controller.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

@customElement('test-property-editor-ui-search-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

function mockUI(
	overrides: Partial<ManifestPropertyEditorUi> & { alias: string; name: string },
): ManifestPropertyEditorUi {
	const { meta, ...rest } = overrides;
	return {
		type: 'propertyEditorUi',
		meta: {
			label: overrides.name,
			icon: 'icon-circle',
			group: 'Common',
			propertyEditorSchemaAlias: 'Umbraco.Plain',
			...meta,
		},
		...rest,
	} as ManifestPropertyEditorUi;
}

const mockUIs: Array<ManifestPropertyEditorUi> = [
	mockUI({
		alias: 'Umb.PropertyEditorUi.TextBox',
		name: 'Text Box',
		meta: { label: 'Text Box', icon: 'icon-edit', group: 'Common', propertyEditorSchemaAlias: 'Umbraco.TextBox' },
	}),
	mockUI({
		alias: 'Umb.PropertyEditorUi.TextArea',
		name: 'Text Area',
		meta: { label: 'Text Area', icon: 'icon-edit', group: 'Common', propertyEditorSchemaAlias: 'Umbraco.TextArea' },
	}),
	mockUI({
		alias: 'Umb.PropertyEditorUi.RichText',
		name: 'Rich Text Editor',
		meta: {
			label: 'Rich Text Editor',
			icon: 'icon-browser-window',
			group: 'richContent',
			propertyEditorSchemaAlias: 'Umbraco.RichText',
		},
	}),
	mockUI({
		alias: 'Umb.PropertyEditorUi.MediaPicker',
		name: 'Media Picker',
		meta: {
			label: 'Media Picker',
			icon: 'icon-picture',
			group: 'Pickers',
			propertyEditorSchemaAlias: 'Umbraco.MediaPicker3',
			keywords: ['image', 'photo', 'picture'],
		},
	}),
	mockUI({
		alias: 'Umb.PropertyEditorUi.ColorPicker',
		name: 'Color Picker',
		meta: {
			label: 'Color Picker',
			icon: 'icon-palette',
			group: 'Pickers',
			propertyEditorSchemaAlias: 'Umbraco.ColorPicker',
		},
	}),
];

describe('UmbPropertyEditorUISearchController', () => {
	let host: UmbTestControllerHostElement;
	let controller: UmbPropertyEditorUISearchController;

	beforeEach(() => {
		host = new UmbTestControllerHostElement();
		controller = new UmbPropertyEditorUISearchController(host);
		controller.setPropertyEditorUIs(mockUIs);
	});

	it('should return empty array for empty query', async () => {
		expect(await controller.search('')).to.deep.equal([]);
		expect(await controller.search('   ')).to.deep.equal([]);
	});

	it('should match by label substring', async () => {
		const results = await controller.search('Text Box');
		expect(results[0].alias).to.equal('Umb.PropertyEditorUi.TextBox');
	});

	it('should match by group', async () => {
		const results = await controller.search('pickers');
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.MediaPicker')).to.be.true;
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.ColorPicker')).to.be.true;
	});

	it('should include fuzzy matches for typos', async () => {
		const results = await controller.search('texbx');
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.TextBox')).to.be.true;
	});

	it('should include fuzzy matches for misspelled group', async () => {
		const results = await controller.search('pickrs');
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.MediaPicker')).to.be.true;
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.ColorPicker')).to.be.true;
	});

	it('should return no results for unrelated query', async () => {
		const results = await controller.search('xyznonexistent');
		expect(results).to.have.length(0);
	});

	it('should match multiple tokens', async () => {
		const results = await controller.search('color picker');
		expect(results[0].alias).to.equal('Umb.PropertyEditorUi.ColorPicker');
	});

	it('should surface partial matches for multi-word queries where only one token matches a keyword', async () => {
		// "hero" matches nothing, but "image" is an exact keyword on Media Picker.
		// Without partial-token scoring this would return zero matches.
		const results = await controller.search('hero image');
		expect(results.some((r) => r.alias === 'Umb.PropertyEditorUi.MediaPicker')).to.be.true;
	});

	it('should abort a prior in-flight search when a new one starts', async () => {
		const many: Array<ManifestPropertyEditorUi> = [];
		for (let i = 0; i < 200; i++) {
			many.push(
				mockUI({
					alias: `Umb.PropertyEditorUi.Bulk${i}`,
					name: `Bulk ${i}`,
				}),
			);
		}
		many.push(...mockUIs);
		controller.setPropertyEditorUIs(many);

		const first = controller.search('Text Box');
		const second = controller.search('Media Picker');

		let firstError: unknown;
		try {
			await first;
		} catch (err) {
			firstError = err;
		}
		expect((firstError as DOMException)?.name).to.equal('AbortError');

		const secondResults = await second;
		expect(secondResults[0].alias).to.equal('Umb.PropertyEditorUi.MediaPicker');
	});

	it('should abort an in-flight search on destroy', async () => {
		const many: Array<ManifestPropertyEditorUi> = [];
		for (let i = 0; i < 200; i++) {
			many.push(
				mockUI({
					alias: `Umb.PropertyEditorUi.Bulk${i}`,
					name: `Bulk ${i}`,
				}),
			);
		}
		many.push(...mockUIs);
		controller.setPropertyEditorUIs(many);

		const pending = controller.search('Text Box');
		controller.destroy();

		let error: unknown;
		try {
			await pending;
		} catch (err) {
			error = err;
		}
		expect((error as DOMException)?.name).to.equal('AbortError');
	});
});
