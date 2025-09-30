import { getDisplayStateFromUserStatus } from './utils.js';
import { expect } from '@open-wc/testing';
import type { UUIInterfaceColor, UUIInterfaceLook } from '@umbraco-cms/backoffice/external/uui';
import { UserStateModel } from '@umbraco-cms/backoffice/external/backend-api';

describe('UmbUserExtensions', () => {
	it('returns correct look and color from a status string', () => {
		const testCases: { status: UserStateModel; look: UUIInterfaceLook; color: UUIInterfaceColor }[] = [
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
