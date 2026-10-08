import { expect } from '@open-wc/testing';
import { ufm } from './marked-ufm.plugin.js';
import { UmbMarked } from '../contexts/ufm.context.js';
import { UmbUfmContentNameComponent } from '../components/content-name/content-name.component.js';
import { UmbUfmElementNameComponent } from '../components/element-name/element-name.component.js';
import { UmbUfmLabelValueComponent } from '../components/label-value/label-value.component.js';
import { UmbUfmLocalizeComponent } from '../components/localize/localize.component.js';
import { UmbUfmMemberNameComponent } from '../components/member-name/member-name.component.js';

describe('UmbMarkedUfm', () => {
	describe('UFM parsing', () => {
		const runs = [
			{ ufm: '{=prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{= prop1 }', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{{=prop1}}', expected: '{<ufm-label-value alias="prop1"></ufm-label-value>}' },
			{
				ufm: '{= prop1 | strip-html | truncate:30}',
				expected: '<ufm-label-value alias="prop1" filters="strip-html | truncate:30"></ufm-label-value>',
			},
			{ ufm: '{umbValue:prop1}', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{ umbValue:prop1 }', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{ umbValue: prop1 }', expected: '<ufm-label-value alias="prop1"></ufm-label-value>' },
			{ ufm: '{#general_add}', expected: '<ufm-localize alias="general_add"></ufm-localize>' },
			{ ufm: '{umbLocalize:general_add}', expected: '<ufm-localize alias="general_add"></ufm-localize>' },
			{ ufm: '{~contentPicker}', expected: '<ufm-content-name alias="contentPicker"></ufm-content-name>' },
			{
				ufm: '{umbContentName: contentPicker}',
				expected: '<ufm-content-name alias="contentPicker"></ufm-content-name>',
			},
			{ ufm: '{umbElementName:elementPicker}', expected: '<ufm-element-name alias="elementPicker"></ufm-element-name>' },
			{
				ufm: '{umbMemberName: memberPicker}',
				expected: '<ufm-member-name alias="memberPicker"></ufm-member-name>',
			},
		];

		// Manually configuring the UFM components for testing.
		UmbMarked.use(
			ufm([
				{ alias: 'umbContentName', marker: '~', render: new UmbUfmContentNameComponent().render },
				{ alias: 'umbElementName', render: new UmbUfmElementNameComponent().render },
				{ alias: 'umbValue', marker: '=', render: new UmbUfmLabelValueComponent().render },
				{ alias: 'umbLocalize', marker: '#', render: new UmbUfmLocalizeComponent().render },
				{ alias: 'umbMemberName', render: new UmbUfmMemberNameComponent().render },
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
