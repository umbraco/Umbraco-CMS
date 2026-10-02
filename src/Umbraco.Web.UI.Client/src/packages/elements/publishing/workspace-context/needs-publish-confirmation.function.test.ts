import { umbNeedsPublishConfirmation } from './needs-publish-confirmation.function.js';
import { expect } from '@open-wc/testing';

describe('umbNeedsPublishConfirmation', () => {
	it('needs confirmation when there is more than one variant, without checking references', async () => {
		let called = false;
		const getReferenceCount = async () => {
			called = true;
			return 0;
		};

		const result = await umbNeedsPublishConfirmation(2, getReferenceCount);

		expect(result).to.equal(true);
		expect(called, 'reference count should not be checked').to.equal(false);
	});

	it('does not need confirmation for a single variant with no references', async () => {
		const result = await umbNeedsPublishConfirmation(1, async () => 0);
		expect(result).to.equal(false);
	});

	it('needs confirmation for a single variant with references', async () => {
		const result = await umbNeedsPublishConfirmation(1, async () => 3);
		expect(result).to.equal(true);
	});

	it('needs confirmation when the reference count lookup fails', async () => {
		const result = await umbNeedsPublishConfirmation(1, async () => {
			throw new Error('lookup failed');
		});
		expect(result).to.equal(true);
	});
});
