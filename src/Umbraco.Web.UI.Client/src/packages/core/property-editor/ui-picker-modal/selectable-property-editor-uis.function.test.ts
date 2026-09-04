import { expect } from '@open-wc/testing';
import { umbSelectablePropertyEditorUis } from './selectable-property-editor-uis.function.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

function mockUI(alias: string, deprecated?: boolean): ManifestPropertyEditorUi {
	return {
		type: 'propertyEditorUi',
		alias,
		name: alias,
		meta: {
			label: alias,
			icon: 'icon-circle',
			group: 'Common',
			propertyEditorSchemaAlias: 'Umbraco.Plain',
			deprecated,
		},
	} as ManifestPropertyEditorUi;
}

describe('umbSelectablePropertyEditorUis', () => {
	it('offers a Property Editor UI that is not deprecated', () => {
		const result = umbSelectablePropertyEditorUis([mockUI('Umb.Test.Current')], []);

		expect(result.map((x) => x.alias)).to.deep.equal(['Umb.Test.Current']);
	});

	it('offers a Property Editor UI that has not declared whether it is deprecated', () => {
		const result = umbSelectablePropertyEditorUis([mockUI('Umb.Test.Undeclared', undefined)], []);

		expect(result.map((x) => x.alias)).to.deep.equal(['Umb.Test.Undeclared']);
	});

	it('does not offer a deprecated Property Editor UI', () => {
		const result = umbSelectablePropertyEditorUis([mockUI('Umb.Test.Current'), mockUI('Umb.Test.Legacy', true)], []);

		expect(result.map((x) => x.alias)).to.deep.equal(['Umb.Test.Current']);
	});

	it('offers a deprecated Property Editor UI while it is the current selection', () => {
		const result = umbSelectablePropertyEditorUis(
			[mockUI('Umb.Test.Current'), mockUI('Umb.Test.Legacy', true)],
			['Umb.Test.Legacy'],
		);

		expect(result.map((x) => x.alias)).to.deep.equal(['Umb.Test.Current', 'Umb.Test.Legacy']);
	});

	it('does not offer a deprecated Property Editor UI when something else is selected', () => {
		const result = umbSelectablePropertyEditorUis(
			[mockUI('Umb.Test.Current'), mockUI('Umb.Test.Legacy', true)],
			['Umb.Test.Current'],
		);

		expect(result.map((x) => x.alias)).to.deep.equal(['Umb.Test.Current']);
	});
});
