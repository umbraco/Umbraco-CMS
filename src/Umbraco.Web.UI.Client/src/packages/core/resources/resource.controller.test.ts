import { UmbResourceController } from './resource.controller.js';
import { UmbApiError, UmbCancelError } from './umb-error.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-resource-controller-host')
class UmbTestResourceControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbResourceController', () => {
	let hostElement: UmbTestResourceControllerHostElement;
	let controller: UmbResourceController<unknown>;

	beforeEach(() => {
		hostElement = new UmbTestResourceControllerHostElement();
		document.body.appendChild(hostElement);
		controller = new UmbResourceController(hostElement, Promise.resolve());
	});

	afterEach(() => {
		hostElement.remove();
	});

	describe('mapToUmbError', () => {
		it('maps a fetch network failure (TypeError) to an UmbApiError with a NetworkError type', () => {
			const networkError = new TypeError('Failed to fetch');

			const result = controller.mapToUmbError(networkError) as UmbApiError;

			expect(UmbApiError.isUmbApiError(result)).to.be.true;
			expect(result.status).to.equal(0);
			expect(result.problemDetails.type).to.equal('NetworkError');
			// The browser's own error message is preserved in the detail for diagnostic/support purposes.
			expect(result.problemDetails.detail).to.include('Failed to fetch');
		});

		it('maps ProblemDetails-like errors to an UmbApiError carrying the original problem details', () => {
			const problemDetails = {
				type: 'ServerError',
				title: 'Boom',
				status: 500,
				detail: 'Something broke',
			};

			const result = controller.mapToUmbError(problemDetails) as UmbApiError;

			expect(result.problemDetails).to.equal(problemDetails);
		});

		it('maps a CancelError to an UmbCancelError', () => {
			const cancelError = new Error('Request aborted');
			cancelError.name = 'CancelError';

			const result = controller.mapToUmbError(cancelError);

			expect(UmbCancelError.isUmbCancelError(result)).to.be.true;
		});

		it('falls back to a generic Unknown error for unrecognizable non-Error values', () => {
			const result = controller.mapToUmbError('some random string') as UmbApiError;

			expect(result.problemDetails.type).to.equal('error');
			expect(result.problemDetails.title).to.equal('Unknown error');
		});
	});
});
