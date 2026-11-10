import { getExternalContentFailureReason } from './get-external-content-failure-reason.function.js';
import { expect } from '@open-wc/testing';
import { UmbApiError } from '@umbraco-cms/backoffice/resources';

const createApiError = (status: number) =>
	new UmbApiError('error', status, undefined, { title: 'error', type: 'ApiError', status });

describe('getExternalContentFailureReason', () => {
	it('reports a 404 as deleted', () => {
		expect(getExternalContentFailureReason(createApiError(404))).to.equal('deleted');
	});

	it('reports a 403 as no access', () => {
		expect(getExternalContentFailureReason(createApiError(403))).to.equal('noAccess');
	});

	it('reports nothing for a server error, so it is not mistaken for an unavailable element', () => {
		expect(getExternalContentFailureReason(createApiError(500))).to.equal(undefined);
	});

	it('reports nothing for an error that is not an API error', () => {
		expect(getExternalContentFailureReason(new Error('network'))).to.equal(undefined);
	});

	it('reports nothing when there is no error', () => {
		expect(getExternalContentFailureReason(undefined)).to.equal(undefined);
	});
});
