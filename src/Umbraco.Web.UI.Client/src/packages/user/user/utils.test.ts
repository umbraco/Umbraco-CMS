import { expect } from '@open-wc/testing';
import { getDisplayStateFromUserStatus } from './utils.js';
import { InterfaceColor, InterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { UserStateModel } from '@umbraco-cms/backoffice/backend-api';

describe('UmbUserExtensions', () => {
	it('returns correct look and color from a status string', () => {
		const testCases: { status: UserStateModel; look: InterfaceLook; color: InterfaceColor }[] = [
			{ status: UserStateModel.ACTIVE, look: 'primary', color: 'positive' },
			{ status: UserStateModel.INACTIVE, look: 'primary', color: 'warning' },
			{ status: UserStateModel.INVITED, look: 'primary', color: 'warning' },
			{ status: UserStateModel.DISABLED, look: 'primary', color: 'danger' },
		];

		testCases.forEach((testCase) => {
			const { look, color } = getDisplayStateFromUserStatus(testCase.status);
			expect(look).to.equal(testCase.look);
			expect(color).to.equal(testCase.color);
		});
	});
});
