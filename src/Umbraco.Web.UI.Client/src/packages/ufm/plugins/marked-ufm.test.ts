import { expect } from '@open-wc/testing';
import { ufm } from './marked-ufm.plugin.js';
import { UmbMarked } from '../index.js';
import { UmbUfmLabelValueComponent } from '../ufm-components/label-value.component.js';
import { UmbUfmLocalizeComponent } from '../ufm-components/localize.component.js';

describe('UmbMarkedUfm', () => {
	describe('UFM parsing', () => {
		const runs = [
			{ ufm: '{=prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1 }', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{{=prop1}}', expected: '{<ufm-label-value alias="prop1"></ufm-label-value>}' },
			{ ufm: '{#general_add}', expected: '<umb-localize key="general_add"></umb-localize>' },
		];

		// Manually configuring the UFM components for testing.
		UmbMarked.use(
			ufm([
				{ alias: 'Umb.Markdown.LabelValue', marker: '=', render: new UmbUfmLabelValueComponent().render },
				{ alias: 'Umb.Markdown.Localize', marker: '#', render: new UmbUfmLocalizeComponent().render },
			]),
		);

		runs.forEach((run) => {
			it(`Parsing "${run.ufm}"`, async () => {
				const markup = await UmbMarked.parseInline(run.ufm);
				expect(markup).to.equal(run.expected);
			});
		});
	});
});
