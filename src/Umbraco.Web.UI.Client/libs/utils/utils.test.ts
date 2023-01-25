import { expect } from '@open-wc/testing';
import { InterfaceColor, InterfaceLook } from '@umbraco-ui/uui-base/lib/types';
import { getTagLookAndColor } from './utils';
import type { UserStatus } from '@umbraco-cms/models';

describe('UmbUserExtensions', () => {
	it('returns correct look and color from a status string', () => {
		const testCases: { status: UserStatus; look: InterfaceLook; color: InterfaceColor }[] = [
			{ status: 'enabled', look: 'primary', color: 'positive' },
			{ status: 'inactive', look: 'primary', color: 'warning' },
			{ status: 'invited', look: 'primary', color: 'warning' },
			{ status: 'disabled', look: 'primary', color: 'danger' },
		];

		testCases.forEach((testCase) => {
			const { look, color } = getTagLookAndColor(testCase.status);
			expect(look).to.equal(testCase.look);
			expect(color).to.equal(testCase.color);
		});
	});
});
