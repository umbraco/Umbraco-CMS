import { expect } from '@open-wc/testing';
import { ufm } from './marked-ufm.plugin.js';
import { UmbMarked } from '../contexts/ufm.context.js';
import { UmbUfmContentNameComponent } from '../components/content-name/content-name.component.js';
import { UmbUfmLabelValueComponent } from '../components/label-value/label-value.component.js';
import { UmbUfmLocalizeComponent } from '../components/localize/localize.component.js';

describe('UmbMarkedUfm', () => {
	describe('UFM parsing', () => {
		const runs = [
			{ ufm: '{=prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1 }', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{{=prop1}}', expected: '{<ufm-label-value alias="prop1"></ufm-label-value>}' },
			{
				ufm: '{= prop1 | strip-html | truncate:30}',
				expected: '<ufm-label-value filters="strip-html | truncate:30" alias="prop1"></ufm-label-value>',
			},
			{ ufm: '{umbValue:prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{#general_add}', expected: '<ufm-localize alias="general_add"></ufm-localize>' },
			{ ufm: '{umbLocalize:general_add}', expected: '<ufm-localize alias="general_add"></ufm-localize>' },
			{ ufm: '{~contentPicker}', expected: '<ufm-content-name alias="contentPicker"></ufm-content-name>' },
			{
				ufm: '{umbContentName: contentPicker}',
				expected: '<ufm-content-name alias="contentPicker"></ufm-content-name>',
			},
		];

		// Manually configuring the UFM components for testing.
		UmbMarked.use(
			ufm([
				{ alias: 'umbContentName', marker: '~', render: new UmbUfmContentNameComponent().render },
				{ alias: 'umbValue', marker: '=', render: new UmbUfmLabelValueComponent().render },
				{ alias: 'umbLocalize', marker: '#', render: new UmbUfmLocalizeComponent().render },
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
